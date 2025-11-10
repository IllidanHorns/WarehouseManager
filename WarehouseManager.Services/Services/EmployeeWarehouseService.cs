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
using WarehouseManagerContracts.DTOs.EmployeeWarehouse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WarehouseManager.Application.Services
{
    public class EmployeeWarehouseService : IEmployeeWarehouseService
    {
        private readonly AppDbContext _context;
        private readonly IAuditService _auditService;

        public EmployeeWarehouseService(AppDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<EmployeeWarehouseSummary> CreateAsync(CreateEmployeeWarehouseCommand command)
        {
            var validator = new WarehouseManagerContracts.Validation.EmployeeWarehouse.CreateEmployeeWarehouseCommandValidator();
            var result = await validator.ValidateAsync(command);
            if (!result.IsValid)
                throw new ModelValidationException(result.Errors.ToList());

            // Проверяем существование сотрудника и склада
            await new EntityCheckHelper(_context).EnsureExistsAndActive<Employee>(command.EmployeeId);
            await new EntityCheckHelper(_context).EnsureExistsAndActive<Warehouse>(command.WarehouseId);

            // Проверяем, нет ли уже активного назначения для этой пары
            var existing = await _context.EmployeesWarehouses
                .FirstOrDefaultAsync(ew => ew.EmployeeId == command.EmployeeId && 
                                          ew.WarehouseId == command.WarehouseId && 
                                          !ew.IsArchived);

            if (existing != null)
                throw new ConflictException("Сотрудник уже назначен на этот склад");

            var employeeWarehouse = new EmployeesWarehouses
            {
                EmployeeId = command.EmployeeId,
                WarehouseId = command.WarehouseId,
                IsArchived = false,
                CreationDateTime = DateTime.UtcNow,
                UpdateDateTime = DateTime.UtcNow
            };

            _context.EmployeesWarehouses.Add(employeeWarehouse);
            await _context.SaveChangesAsync();

            _auditService.LogOperation(
                userId: command.UserId,
                action: $"Назначение сотрудника ID={command.EmployeeId} на склад ID={command.WarehouseId}",
                tableName: "EmployeesWarehouses",
                recordId: employeeWarehouse.Id.ToString()
            );

            return await MapToSummaryAsync(employeeWarehouse);
        }

        public async Task<bool> ArchiveAsync(int employeeWarehouseId, int currentUserId)
        {
            var employeeWarehouse = await new EntityCheckHelper(_context).EnsureExistsAndActive<EmployeesWarehouses>(employeeWarehouseId);

            employeeWarehouse.IsArchived = true;
            employeeWarehouse.UpdateDateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _auditService.LogOperation(
                userId: currentUserId,
                action: $"Снятие назначения сотрудника ID={employeeWarehouse.EmployeeId} со склада ID={employeeWarehouse.WarehouseId}",
                tableName: "EmployeesWarehouses",
                recordId: employeeWarehouse.Id.ToString()
            );

            return true;
        }
        public async Task<PagedResult<EmployeeWarehouseSummary>> GetPagedAsync(IPaginationFilter filter)
        {
            IQueryable<EmployeesWarehouses> query = _context.EmployeesWarehouses
                .Include(ew => ew.Employee)
                    .ThenInclude(e => e.User)
                .Include(ew => ew.Warehouse);

            if (!filter.IncludeArchived)
            {
                query = query.Where(ew => !ew.IsArchived);
            }

            query = ApplyCustomFilters(query, filter);

            var totalCount = await query.CountAsync();

            var pagedItems = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var summaries = await MapToSummariesAsync(pagedItems);

            return new PagedResult<EmployeeWarehouseSummary>
            {
                Items = summaries,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        private IQueryable<EmployeesWarehouses> ApplyCustomFilters(IQueryable<EmployeesWarehouses> query, IPaginationFilter filter)
        {
            if (filter is not EmployeeWarehouseFilter employeeWarehouseFilter)
                return query;

            if (employeeWarehouseFilter.EmployeeId.HasValue)
            {
                query = query.Where(ew => ew.EmployeeId == employeeWarehouseFilter.EmployeeId.Value);
            }

            if (employeeWarehouseFilter.WarehouseId.HasValue)
            {
                query = query.Where(ew => ew.WarehouseId == employeeWarehouseFilter.WarehouseId.Value);
            }

            return query;
        }

        private async Task<EmployeeWarehouseSummary> MapToSummaryAsync(EmployeesWarehouses entity)
        {
            await _context.Entry(entity).Reference(ew => ew.Employee).LoadAsync();
            await _context.Entry(entity).Reference(ew => ew.Warehouse).LoadAsync();
            await _context.Entry(entity.Employee).Reference(e => e.User).LoadAsync();

            var user = entity.Employee.User;
            var fullName = user != null 
                ? $"{user.FirstName} {user.MiddleName} {(string.IsNullOrEmpty(user.Patronymic) ? "" : user.Patronymic)}"
                : "Неизвестен";

            return new EmployeeWarehouseSummary
            {
                Id = entity.Id,
                EmployeeId = entity.EmployeeId,
                EmployeeFullName = fullName,
                EmployeeEmail = user?.Email ?? "Нет email",
                WarehouseId = entity.WarehouseId,
                WarehouseAddress = entity.Warehouse?.Address ?? "Неизвестен",
                CreationDateTime = entity.CreationDateTime,
                IsArchived = entity.IsArchived
            };
        }

        private async Task<List<EmployeeWarehouseSummary>> MapToSummariesAsync(List<EmployeesWarehouses> entities)
        {
            var employeeIds = entities.Select(ew => ew.EmployeeId).Distinct().ToList();
            var warehouseIds = entities.Select(ew => ew.WarehouseId).Distinct().ToList();

            var employees = await _context.Employees
                .Include(e => e.User)
                .Where(e => employeeIds.Contains(e.Id))
                .ToListAsync();

            var warehouses = await _context.Warehouses
                .Where(w => warehouseIds.Contains(w.Id))
                .ToListAsync();

            var employeeDict = employees.ToDictionary(e => e.Id);
            var warehouseDict = warehouses.ToDictionary(w => w.Id);

            return entities.Select(ew =>
            {
                var employee = employeeDict.GetValueOrDefault(ew.EmployeeId);
                var user = employee?.User;
                var fullName = user != null
                    ? $"{user.FirstName} {user.MiddleName} {(string.IsNullOrEmpty(user.Patronymic) ? "" : user.Patronymic)}"
                    : "Неизвестен";

                return new EmployeeWarehouseSummary
                {
                    Id = ew.Id,
                    EmployeeId = ew.EmployeeId,
                    EmployeeFullName = fullName,
                    EmployeeEmail = user?.Email ?? "Нет email",
                    WarehouseId = ew.WarehouseId,
                    WarehouseAddress = warehouseDict.GetValueOrDefault(ew.WarehouseId)?.Address ?? "Неизвестен",
                    CreationDateTime = ew.CreationDateTime,
                    IsArchived = ew.IsArchived
                };
            }).ToList();
        }
    }
}

