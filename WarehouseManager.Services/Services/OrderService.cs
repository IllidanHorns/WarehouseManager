using Microsoft.EntityFrameworkCore;
using WarehouseManager.Core.Data;
using WarehouseManager.Core.Models;
using WarehouseManager.Services;
using WarehouseManager.Services.Exceptions;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Filters.Interfaces;
using WarehouseManager.Services.Helpers;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManagerContracts.DTOs.Order;

namespace WarehouseManager.Application.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ITransactionManager _transactionManager;
    private readonly IMetricsService? _metricsService;

    public OrderService(AppDbContext context, IAuditService auditService, ITransactionManager transactionManager, IMetricsService? metricsService = null)
    {
        _context = context;
        _auditService = auditService;
        _transactionManager = transactionManager;
        _metricsService = metricsService;
    }

    public async Task<OrderSummary> CreateAsync(CreateOrderCommand command)
    {
        // Проверка существования склада
        await new EntityCheckHelper(_context).EnsureExistsAndActive<Warehouse>(command.WarehouseId);

        // Проверка существования пользователя
        await new EntityCheckHelper(_context).EnsureExistsAndActive<User>(command.UserId);

        // Находим статус "Активен" по имени
        var activeStatus = await _context.OrderStatuses
            .FirstOrDefaultAsync(s => s.StatusName == "Активен" && !s.IsArchived);
        
        if (activeStatus == null)
        {
            throw new DomainException("Статус 'Активен' не найден в базе данных.");
        }

        // Проверка продуктов
        var productIds = command.Products.Select(p => p.ProductId).Distinct().ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id) && !p.IsArchived)
            .ToDictionaryAsync(p => p.Id);

        if (products.Count != productIds.Count)
        {
            var missingIds = productIds.Except(products.Keys).ToList();
            throw new DomainException($"Продукты с ID {string.Join(", ", missingIds)} не найдены или архивированы.");
        }

        // Получаем остатки на складе
        var remainingStock = await _context.Remaining
            .Where(r => r.WarehouseId == command.WarehouseId && 
                       productIds.Contains(r.ProductId) && 
                       !r.IsArchived)
            .ToDictionaryAsync(r => r.ProductId, r => r.Quantity);

        // Проверяем наличие и количество каждого продукта
        foreach (var item in command.Products)
        {
            var product = products[item.ProductId];

            if (item.Quantity <= 0)
                throw new DomainException($"Количество товара '{product.ProductName}' должно быть больше 0.");

            if (item.OrderPrice != product.Price)
                throw new ConflictException($"Цена товара '{product.ProductName}' не совпадает. Ожидалось: {product.Price}, получено: {item.OrderPrice}.");

            if (!remainingStock.TryGetValue(item.ProductId, out var available) || available < item.Quantity)
                throw new ConflictException($"Недостаточно товара '{product.ProductName}' на складе. Доступно: {available}, запрошено: {item.Quantity}.");
        }

        return await _transactionManager.ExecuteOrderWorkflowAsync(async () =>
        {
            var totalPrice = command.Products.Sum(p => p.Quantity * p.OrderPrice);

            int? defaultEmployeeId = null;
            var employeeOnWarehouse = await _context.EmployeesWarehouses
                .Include(ew => ew.Employee)
                .Where(ew => ew.WarehouseId == command.WarehouseId && 
                            !ew.IsArchived && 
                            !ew.Employee.IsArchived)
                .Select(ew => (int?)ew.EmployeeId)
                .FirstOrDefaultAsync();

            if (employeeOnWarehouse != null)
            {
                defaultEmployeeId = employeeOnWarehouse.Value;
            }
            else
            {
                var anyEmployee = await _context.Employees
                    .Where(e => !e.IsArchived)
                    .Select(e => (int?)e.Id)
                    .FirstOrDefaultAsync();

                if (anyEmployee != null)
                {
                    defaultEmployeeId = anyEmployee.Value;
                }
            }

            var order = new Order
            {
                WarehouseId = command.WarehouseId,
                EmployeeId = defaultEmployeeId,
                UserId = command.UserId,
                StatusId = activeStatus.Id,
                TotalPrice = totalPrice,
                CreationDatetime = DateTime.UtcNow,
                UpdateDatetime = DateTime.UtcNow,
                IsArchived = false
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var orderProducts = command.Products.Select(p => new OrdersProducts
            {
                OrderId = order.Id,
                ProductId = p.ProductId,
                Quantity = p.Quantity,
                OrderPrice = p.OrderPrice,
                TotalPrice = p.Quantity * p.OrderPrice,
                CreationDatetime = DateTime.UtcNow,
                UpdateDatetime = DateTime.UtcNow,
                IsArchived = false
            }).ToList();

            _context.OrdersProducts.AddRange(orderProducts);

            foreach (var item in command.Products)
            {
                var remaining = await _context.Remaining
                    .FirstAsync(r => r.WarehouseId == command.WarehouseId && 
                                   r.ProductId == item.ProductId && 
                                   !r.IsArchived);
                remaining.Quantity -= item.Quantity;
                remaining.UpdateDatetime = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Увеличиваем счетчик созданных заказов
            if (_metricsService != null)
            {
                await _metricsService.IncrementMetricAsync("TotalOrdersCreated");
            }

            _auditService.LogOperation(
                userId: command.UserId,
                action: $"Создание заказа ID={order.Id}, сумма: {totalPrice} руб.",
                tableName: "Orders",
                recordId: order.Id.ToString()
            );

            await _context.SaveChangesAsync();

            var warehouse = await _context.Warehouses.FindAsync(order.WarehouseId);
            var user = await _context.Users.FindAsync(order.UserId);

            return new OrderSummary
            {
                Id = order.Id,
                TotalPrice = order.TotalPrice,
                CreationDatetime = order.CreationDatetime,
                WarehouseAddress = warehouse?.Address ?? "Неизвестно",
                EmployeeFullName = "Не назначен",
                UserEmail = user?.Email ?? "Неизвестно",
                OrderStatusName = activeStatus.StatusName
            };
        });
    }

    public async Task<PagedResult<OrderSummary>> GetPagedAsync(IPaginationFilter filter)
    {
        IQueryable<Order> query = _context.Orders
            .Include(o => o.Warehouse)
            .Include(o => o.Employee)
                .ThenInclude(e => e.User)
            .Include(o => o.User)
            .Include(o => o.OrderStatus);

        if (!filter.IncludeArchived)
        {
            query = query.Where(o => !o.IsArchived);
        }

        query = ApplyCustomFilters(query, filter);

        var totalCount = await query.CountAsync();

        var pagedItems = await query
            .OrderByDescending(o => o.CreationDatetime)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var summaries = await MapToSummariesAsync(pagedItems);

        return new PagedResult<OrderSummary>
        {
            Items = summaries,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<OrderSummary> GetByIdAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Warehouse)
            .Include(o => o.Employee)
                .ThenInclude(e => e.User)
            .Include(o => o.User)
            .Include(o => o.OrderStatus)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null || order.IsArchived)
            throw new DomainException($"Заказ с ID {orderId} не найден или архивирован");

        return await MapToSummaryAsync(order);
    }

    public async Task<OrderSummary> UpdateStatusAsync(UpdateOrderStatusCommand command)
    {
        var order = await _context.Orders
            .Include(o => o.OrderStatus)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId);
        if (order == null || order.IsArchived)
            throw new DomainException($"Заказ с ID {command.OrderId} не найден или архивирован");
        
        var status = await new EntityCheckHelper(_context).EnsureExistsAndActive<OrderStatus>(command.StatusId);

        var oldStatusName = order.OrderStatus?.StatusName ?? "Неизвестно";
        
        order.StatusId = command.StatusId;
        order.UpdateDatetime = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        if (command.UserId.HasValue)
        {
            _auditService.LogOperation(
                userId: command.UserId.Value,
                action: $"Обновление статуса заказа ID={order.Id} с '{oldStatusName}' на '{status.StatusName}'",
                tableName: "Orders",
                recordId: order.Id.ToString()
            );
        }

        return await MapToSummaryAsync(order);
    }

    public async Task<OrderSummary> AssignEmployeeAsync(AssignEmployeeToOrderCommand command)
    {
        var order = await _context.Orders
            .Include(o => o.Employee)
                .ThenInclude(e => e.User)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId);
        if (order == null || order.IsArchived)
            throw new DomainException($"Заказ с ID {command.OrderId} не найден или архивирован");
        var employee = await new EntityCheckHelper(_context).EnsureExistsAndActive<Employee>(command.EmployeeId);

        // Проверяем, что сотрудник работает на складе заказа (не обязательно, но желательно)
        var employeeWorksOnWarehouse = await _context.EmployeesWarehouses
            .AnyAsync(ew => ew.EmployeeId == command.EmployeeId && 
                           ew.WarehouseId == order.WarehouseId && 
                           !ew.IsArchived);

        if (!employeeWorksOnWarehouse)
        {
            // Можно выдать предупреждение, но не блокировать
            // throw new DomainException("Сотрудник не работает на складе заказа");
        }

        var oldEmployeeName = order.Employee?.User != null 
            ? $"{order.Employee.User.FirstName} {order.Employee.User.MiddleName}" 
            : "Не назначен";

        order.EmployeeId = command.EmployeeId;
        order.UpdateDatetime = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        if (command.UserId.HasValue)
        {
            var newEmployeeName = $"{employee.User.FirstName} {employee.User.MiddleName}";
            _auditService.LogOperation(
                userId: command.UserId.Value,
                action: $"Назначение сотрудника '{newEmployeeName}' на заказ ID={order.Id} (было: '{oldEmployeeName}')",
                tableName: "Orders",
                recordId: order.Id.ToString()
            );
        }

        return await MapToSummaryAsync(order);
    }

    private IQueryable<Order> ApplyCustomFilters(IQueryable<Order> query, IPaginationFilter filter)
    {
        if (filter is not OrderFilter orderFilter)
            return query;

        if (orderFilter.WarehouseId.HasValue)
        {
            query = query.Where(o => o.WarehouseId == orderFilter.WarehouseId.Value);
        }

        if (orderFilter.EmployeeId.HasValue)
        {
            query = query.Where(o => o.EmployeeId == orderFilter.EmployeeId.Value);
        }

        if (orderFilter.StatusId.HasValue)
        {
            query = query.Where(o => o.StatusId == orderFilter.StatusId.Value);
        }

        if (orderFilter.UserId.HasValue)
        {
            query = query.Where(o => o.UserId == orderFilter.UserId.Value);
        }

        return query;
    }

    private async Task<OrderSummary> MapToSummaryAsync(Order order)
    {
        // Загружаем связанные данные, если они еще не загружены
        if (!_context.Entry(order).Reference(o => o.Warehouse).IsLoaded)
            await _context.Entry(order).Reference(o => o.Warehouse).LoadAsync();
        
        // Загружаем Employee только если EmployeeId не null
        if (order.EmployeeId.HasValue && !_context.Entry(order).Reference(o => o.Employee).IsLoaded)
            await _context.Entry(order).Reference(o => o.Employee).LoadAsync();
        
        if (!_context.Entry(order).Reference(o => o.User).IsLoaded)
            await _context.Entry(order).Reference(o => o.User).LoadAsync();
        if (!_context.Entry(order).Reference(o => o.OrderStatus).IsLoaded)
            await _context.Entry(order).Reference(o => o.OrderStatus).LoadAsync();

        if (order.Employee != null && !_context.Entry(order.Employee).Reference(e => e.User).IsLoaded)
        {
            await _context.Entry(order.Employee).Reference(e => e.User).LoadAsync();
        }

        var employeeFullName = order.Employee?.User != null
            ? $"{order.Employee.User.FirstName} {order.Employee.User.MiddleName} {(string.IsNullOrEmpty(order.Employee.User.Patronymic) ? "" : order.Employee.User.Patronymic)}"
            : "Не назначен";

        return new OrderSummary
        {
            Id = order.Id,
            TotalPrice = order.TotalPrice,
            CreationDatetime = order.CreationDatetime,
            WarehouseAddress = order.Warehouse?.Address ?? "Неизвестно",
            EmployeeFullName = employeeFullName,
            UserEmail = order.User?.Email ?? "Неизвестно",
            OrderStatusName = order.OrderStatus?.StatusName ?? "Неизвестно"
        };
    }

    private async Task<List<OrderSummary>> MapToSummariesAsync(List<Order> orders)
    {
        var warehouseIds = orders.Select(o => o.WarehouseId).Distinct().ToList();
        var employeeIds = orders.Where(o => o.EmployeeId.HasValue).Select(o => o.EmployeeId!.Value).Distinct().ToList();
        var userIds = orders.Select(o => o.UserId).Distinct().ToList();
        var statusIds = orders.Select(o => o.StatusId).Distinct().ToList();

        var warehouses = await _context.Warehouses
            .Where(w => warehouseIds.Contains(w.Id))
            .ToDictionaryAsync(w => w.Id);

        var employees = await _context.Employees
            .Include(e => e.User)
            .Where(e => employeeIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id);

        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        var statuses = await _context.OrderStatuses
            .Where(s => statusIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id);

        return orders.Select(order =>
        {
            var warehouse = warehouses.GetValueOrDefault(order.WarehouseId);
            var employee = order.EmployeeId.HasValue ? employees.GetValueOrDefault(order.EmployeeId.Value) : null;
            var user = users.GetValueOrDefault(order.UserId);
            var status = statuses.GetValueOrDefault(order.StatusId);

            var employeeFullName = employee?.User != null
                ? $"{employee.User.FirstName} {employee.User.MiddleName} {(string.IsNullOrEmpty(employee.User.Patronymic) ? "" : employee.User.Patronymic)}"
                : "Не назначен";

            return new OrderSummary
            {
                Id = order.Id,
                TotalPrice = order.TotalPrice,
                CreationDatetime = order.CreationDatetime,
                WarehouseAddress = warehouse?.Address ?? "Неизвестно",
                EmployeeFullName = employeeFullName,
                UserEmail = user?.Email ?? "Неизвестно",
                OrderStatusName = status?.StatusName ?? "Неизвестно"
            };
        }).ToList();
    }
}

