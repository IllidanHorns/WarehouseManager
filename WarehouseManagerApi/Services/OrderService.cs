/*// Services/Implementations/OrderService.cs
using Microsoft.EntityFrameworkCore;

using WarehouseManagerApi.Data;
using WarehouseManagerApi.Models;
using WarehouseManagerApi.Services.Interface;
using WarehouseManagerContracts.DTOs.Order;
using WarehouseManagerContracts.DTOs.OrderProduct;

namespace WarehouseManagerApi.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderDto>> GetAllAsync(bool includeArchived = false)
    {
        var query = _context.Orders
            .Include(o => o.Warehouse)
            .Include(o => o.Employee)
            .Include(o => o.User)
            .Include(o => o.OrderStatus)
            .Include(o => o.OrdersProducts).ThenInclude(op => op.Product)
            .AsQueryable();

        if (!includeArchived)
            query = query.Where(o => !o.IsArchived);

        var orders = await query.OrderByDescending(o => o.CreationDatetime).ToListAsync();
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Warehouse)
            .Include(o => o.Employee)
            .Include(o => o.User)
            .Include(o => o.OrderStatus)
            .Include(o => o.OrdersProducts).ThenInclude(op => op.Product)
            .FirstOrDefaultAsync(o => o.OrderId == id && !o.IsArchived);

        return order == null ? null : MapToDto(order);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto)
    {
        var warehouseExists = await _context.Warehouses.AnyAsync(w => w.WarehouseId == dto.WarehouseId && !w.IsArchived);
        if (!warehouseExists)
            throw new InvalidOperationException("Warehouse does not exist or is archived.");

            var employeeExists = await _context.Employees.AnyAsync(e => e.EmployeeId == dto.EmployeeId && !e.IsArchived);
            if (!employeeExists)
                throw new InvalidOperationException("Employee does not exist or is archived.");

        var userExists = await _context.Users.AnyAsync(u => u.UserId == dto.UserId && !u.IsArchived);
        if (!userExists)
            throw new InvalidOperationException("User does not exist or is archived.");

        var statusExists = await _context.OrderStatuses.AnyAsync(s => s.OrderStatusId == dto.StatusId && !s.IsArchived);
        if (!statusExists)
            throw new InvalidOperationException("Order status does not exist or is archived.");

        var productIds = dto.Products.Select(p => p.ProductId).ToList();
        var productsInDb = await _context.Products
            .Where(p => productIds.Contains(p.ProductId) && !p.IsArchived)
            .ToDictionaryAsync(p => p.ProductId);

        if (productsInDb.Count != productIds.Count)
            throw new InvalidOperationException("One or more products do not exist or are archived.");

        var remainingStock = await _context.Remaining
            .Where(r => r.WarehouseId == dto.WarehouseId && !r.IsArchived)
            .ToDictionaryAsync(r => r.ProductId, r => r.Quantity);

        foreach (var item in dto.Products)
        {
            if (!productsInDb.TryGetValue(item.ProductId, out var product))
                throw new InvalidOperationException($"Product {item.ProductId} not found.");

            if (item.Quantity <= 0)
                throw new InvalidOperationException("Quantity must be greater than 0.");

            if (item.OrderPrice != product.Price)
                throw new InvalidOperationException($"Price mismatch for product {product.ProductName}. Expected: {product.Price}, got: {item.OrderPrice}.");

            if (!remainingStock.TryGetValue(item.ProductId, out var available) || available < item.Quantity)
                throw new InvalidOperationException($"Insufficient stock for product {product.ProductName}. Available: {available}, requested: {item.Quantity}.");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = new Order
            {
                WarehouseId = dto.WarehouseId,
                EmployeeId = dto.EmployeeId,
                UserId = dto.UserId,
                StatusId = dto.StatusId,
                TotalPrice = dto.Products.Sum(p => p.Quantity * p.OrderPrice),
                CreationDatetime = DateTime.UtcNow,
                UpdateDatetime = DateTime.UtcNow,
                IsArchived = false
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); 

            var orderProducts = dto.Products.Select(p => new OrdersProducts
            {
                OrderId = order.OrderId,
                ProductId = p.ProductId,
                Quantity = p.Quantity,
                OrderPrice = p.OrderPrice,
                TotalPrice = p.Quantity * p.OrderPrice,
                CreationDatetime = DateTime.UtcNow,
                UpdateDatetime = DateTime.UtcNow,
                IsArchived = false
            }).ToList();

            _context.OrdersProducts.AddRange(orderProducts);

            // Уменьшаем остатки
            foreach (var item in dto.Products)
            {
                var remaining = await _context.Remaining
                    .FirstAsync(r => r.WarehouseId == dto.WarehouseId && r.ProductId == item.ProductId);
                remaining.Quantity -= item.Quantity;
                remaining.UpdateDatetime = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapToDto(order, orderProducts, productsInDb.Values.ToList());
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<OrderDto?> UpdateStatusAsync(int id, int newStatusId)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id && !o.IsArchived);
        if (order == null) return null;

        var statusExists = await _context.OrderStatuses.AnyAsync(s => s.OrderStatusId == newStatusId && !s.IsArchived);
        if (!statusExists)
            throw new InvalidOperationException("New status does not exist or is archived.");


        order.StatusId = newStatusId;
        order.UpdateDatetime = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var orderWithDetails = await _context.Orders
            .Include(o => o.Warehouse)
            .Include(o => o.Employee)
            .Include(o => o.User)
            .Include(o => o.OrderStatus)
            .Include(o => o.OrdersProducts).ThenInclude(op => op.Product)
            .FirstAsync(o => o.OrderId == id);

        return MapToDto(orderWithDetails);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id && !o.IsArchived);
        if (order == null) return false;

        order.IsArchived = true;
        order.UpdateDatetime = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    private OrderDto MapToDto(Order order)
    {
        var products = order.OrdersProducts?.Select(op => new OrderProductDto
        {
            ProductId = op.ProductId,
            ProductName = op.Product?.ProductName ?? "Unknown",
            Quantity = op.Quantity,
            OrderPrice = op.OrderPrice
        }).ToList() ?? new List<OrderProductDto>();

        return new OrderDto
        {
            OrderId = order.OrderId,
            TotalPrice = order.TotalPrice,
            CreationDatetime = order.CreationDatetime,
            WarehouseAddress = order.Warehouse?.Address ?? "Unknown",
            EmployeeFullName = order.Employee != null
                ? $"{order.Employee.User.FirstName} {order.Employee.User.MiddleName}"
                : null,
            UserEmail = order.User?.Email ?? "Unknown",
            OrderStatusName = order.OrderStatus?.StatusName ?? "Unknown",
            Products = products
        };
    }

    private OrderDto MapToDto(Order order, List<OrdersProducts> orderProducts, List<Product> products)
    {
        var productDict = products.ToDictionary(p => p.ProductId);
        var productDtos = orderProducts.Select(op => new OrderProductDto
        {
            ProductId = op.ProductId,
            ProductName = productDict[op.ProductId].ProductName,
            Quantity = op.Quantity,
            OrderPrice = op.OrderPrice
        }).ToList();

        var warehouse = _context.Warehouses.Local.FirstOrDefault(w => w.WarehouseId == order.WarehouseId)
                         ?? new Warehouse { Address = "Created in transaction" };
        var user = _context.Users.Local.FirstOrDefault(u => u.UserId == order.UserId)
                   ?? new User { Email = "Unknown" };
        var employee =
            _context.Employees.Local.FirstOrDefault(e => e.EmployeeId == order.EmployeeId)
              ?.User;
        var status = _context.OrderStatuses.Local.FirstOrDefault(s => s.OrderStatusId == order.StatusId)
                     ?? new OrderStatus { StatusName = "Unknown" };

        return new OrderDto
        {
            OrderId = order.OrderId,
            TotalPrice = order.TotalPrice,
            CreationDatetime = order.CreationDatetime,
            WarehouseAddress = warehouse.Address,
            EmployeeFullName = employee != null ? $"{employee.FirstName} {employee.MiddleName}" : null,
            UserEmail = user.Email,
            OrderStatusName = status.StatusName,
            Products = productDtos
        };
    }

    public async Task<OrderDto?> AssignEmployeeAsync(int orderId, int employeeId)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.OrderId == orderId && !o.IsArchived);
        if (order == null) return null;

            var employeeExists = await _context.Employees
                .AnyAsync(e => e.EmployeeId == employeeId && !e.IsArchived);
            if (!employeeExists)
                throw new InvalidOperationException("Employee does not exist or is archived.");

        order.EmployeeId = employeeId;
        order.UpdateDatetime = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    
        var orderWithDetails = await _context.Orders
            .Include(o => o.Warehouse)
            .Include(o => o.Employee).ThenInclude(e => e.User)
            .Include(o => o.User)
            .Include(o => o.OrderStatus)
            .Include(o => o.OrdersProducts).ThenInclude(op => op.Product)
            .FirstAsync(o => o.OrderId == orderId);

        return MapToDto(orderWithDetails);
    }
}*/