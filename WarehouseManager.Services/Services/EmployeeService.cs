using Microsoft.EntityFrameworkCore;
using WarehouseManager.Contracts.Validation.Employee;
using WarehouseManager.Core.Data;
using WarehouseManager.Core.Models;
using WarehouseManager.Services.Exceptions;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Filters.Interfaces;
using WarehouseManager.Services.Helpers;
using WarehouseManager.Services.Services.Base;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManagerContracts.DTOs.Employee;
using WarehouseManagerContracts.Validation.Employee;

namespace WarehouseManager.Application.Services;

public class EmployeeService : ArchiveService<Employee, EmployeeSummary>, IEmployeeService
{
    private readonly IAuditService _auditService;

    public EmployeeService(AppDbContext context, IAuditService auditService) : base(context)
    {
        _auditService = auditService;
    }

    protected override async Task<EmployeeSummary> MapToSummaryAsync(Employee entity)
    {
        var user = await _context.Users.FindAsync(entity.UserId);
        var warehouseCount = await _context.EmployeesWarehouses
            .CountAsync(ew => ew.EmployeeId == entity.Id && !ew.IsArchived);
        var orderCount = await _context.Orders
            .CountAsync(o => o.EmployeeId.HasValue && o.EmployeeId.Value == entity.Id && !o.IsArchived);

        return new EmployeeSummary
        {
            Id = entity.Id,
            UserId = entity.UserId,
            FullName = user != null
                ? $"{user.FirstName} {user.MiddleName} {(string.IsNullOrEmpty(user.Patronymic) ? "" : user.Patronymic)}"
                : "Неизвестен",
            Email = user?.Email ?? "Нет email",
            Salary = entity.Salary,
            DateOfBirth = entity.DateOfBirth,
            IsArchived = entity.IsArchived,
            CreationDatetime = entity.CreationDatetime,
            AssignedWarehouseCount = warehouseCount,
            AssignedOrderCount = orderCount
        };
    }

    protected override async Task<List<EmployeeSummary>> MapToSummariesAsync(List<Employee> entities)
    {
        var userIds = entities.Select(e => e.UserId).ToList();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        var employeeIds = entities.Select(e => e.Id).ToList();

        var warehouseCounts = await _context.EmployeesWarehouses
            .Where(ew => employeeIds.Contains(ew.EmployeeId) && !ew.IsArchived)
            .GroupBy(ew => ew.EmployeeId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        var orderCounts = await _context.Orders
            .Where(o => o.EmployeeId.HasValue && employeeIds.Contains(o.EmployeeId.Value) && !o.IsArchived)
            .GroupBy(o => o.EmployeeId!.Value)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        return entities.Select(e => new EmployeeSummary
        {
            Id = e.Id,
            UserId = e.UserId,
            FullName = users.TryGetValue(e.UserId, out var user)
                ? $"{user.FirstName} {user.MiddleName} {(string.IsNullOrEmpty(user.Patronymic) ? "" : user.Patronymic)}"
                : "Неизвестен",
            Email = users.GetValueOrDefault(e.UserId)?.Email ?? "Нет email",
            Salary = e.Salary,
            DateOfBirth = e.DateOfBirth,
            IsArchived = e.IsArchived,
            CreationDatetime = e.CreationDatetime,
            AssignedWarehouseCount = warehouseCounts.GetValueOrDefault(e.Id, 0),
            AssignedOrderCount = orderCounts.GetValueOrDefault(e.Id, 0)
        }).ToList();
    }

    protected override IQueryable<Employee> ApplyCustomFilters(IQueryable<Employee> query, IPaginationFilter filter)
    {
        if (filter is not EmployeeFilters employeeFilter)
            return query;

        if (!employeeFilter.IncludeArchived)
            query = query.Where(e => !e.IsArchived);

        if (employeeFilter.WarehouseId.HasValue)
        {
            var warehouseId = employeeFilter.WarehouseId.Value;
            query = query.Where(e => e.EmployeesWarehouses != null && 
                                     e.EmployeesWarehouses
                .Any(ew => ew.WarehouseId == warehouseId && !ew.IsArchived));
        }
        return query;
    }

    public async Task<EmployeeSummary> CreateAsync(CreateEmployeeCommand command)
    {
        var validator = new CreateEmployeeCommandValidator();
        var result = await validator.ValidateAsync(command);
        if (!result.IsValid)
            throw new ModelValidationException(result.Errors.ToList());

        await new EntityCheckHelper(_context).EnsureExistsAndActive<User>(command.UserId);

        if (await _context.Employees.AnyAsync(e => e.UserId == command.UserId))
            throw new ConflictException("Сотрудник для этого пользователя уже существует");

        var employee = new Employee
        {
            UserId = command.UserId,
            Salary = command.Salary,
            DateOfBirth = command.DateOfBirth,
            IsArchived = false,
            CreationDatetime = DateTime.UtcNow
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        _auditService.LogOperation(
            userId: command.UserId, 
            action: $"Создание сотрудника для пользователя ID={command.UserId}",
            tableName: "Employees",
            recordId: employee.Id.ToString()
        );

        return await MapToSummaryAsync(employee);
    }

    public async Task<EmployeeSummary> UpdateAsync(UpdateEmployeeCommand command)
    {
        var employee = await new EntityCheckHelper(_context).EnsureExistsAndActive<Employee>(command.EmployeeId);

        if (employee.UserId != command.UserId)
        {
            await new EntityCheckHelper(_context).EnsureExistsAndActive<User>(command.UserId);

            if (await _context.Employees.AnyAsync(e => e.UserId == command.UserId && e.Id != command.EmployeeId))
                throw new ConflictException("Этот пользователь уже привязан к другому сотруднику");
        }

        var validator = new UpdateEmployeeCommandValidator();
        var result = await validator.ValidateAsync(command);
        if (!result.IsValid)
            throw new ModelValidationException(result.Errors.ToList());

        var oldUserId = employee.UserId;
        var oldSalary = employee.Salary;

        employee.UserId = command.UserId;
        employee.Salary = command.Salary;
        employee.DateOfBirth = command.DateOfBirth;
        employee.UpdateDatetime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _auditService.LogOperation(
            userId: command.UserId,
            action: $"Обновление сотрудника ID={employee.Id}",
            oldValue: $"User ID: {oldUserId}, Salary: {oldSalary}",
            newValue: $"User ID: {employee.UserId}, Salary: {employee.Salary}",
            tableName: "Employees",
            recordId: employee.Id.ToString()
        );

        return await MapToSummaryAsync(employee);
    }

    public async Task<bool> ArchiveAsync(int targetEmployeeId, int currentUserId)
    {
        var employee = await new EntityCheckHelper(_context).EnsureExistsAndActive<Employee>(targetEmployeeId);

        // Находим статус "Отменён" для проверки активных заказов
        var cancelledStatus = await _context.OrderStatuses
            .FirstOrDefaultAsync(s => s.StatusName == "Отменён" && !s.IsArchived);

        var hasActiveOrders = await _context.Orders
            .AnyAsync(o => o.EmployeeId.HasValue && o.EmployeeId.Value == targetEmployeeId
                           && !o.IsArchived
                           && (cancelledStatus == null || o.StatusId != cancelledStatus.Id));
        if (hasActiveOrders)
            throw new ConflictException("Нельзя архивировать сотрудника: есть активные заказы");

        var hasActiveWarehouses = await _context.EmployeesWarehouses
            .AnyAsync(ew => ew.EmployeeId == targetEmployeeId && !ew.IsArchived);
        if (hasActiveWarehouses)
            throw new ConflictException("Нельзя архивировать сотрудника: есть активные привязки к складам");

        employee.IsArchived = true;
        employee.UpdateDatetime = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _auditService.LogOperation(
            userId: currentUserId,
            action: $"Архивирование сотрудника ID={employee.Id}",
            tableName: "Employees",
            recordId: employee.Id.ToString()
        );

        return true;
    }
}
