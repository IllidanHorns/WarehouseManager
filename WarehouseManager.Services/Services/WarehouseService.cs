using Microsoft.EntityFrameworkCore;
using WarehouseManager.Core.Data;
using WarehouseManager.Core.Models;
using WarehouseManager.Services.Exceptions;
using WarehouseManager.Services.Extensions;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Filters.Interfaces;
using WarehouseManager.Services.Helpers;
using WarehouseManager.Services.Services.Base;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManagerContracts.DTOs.Warehouse;
using WarehouseManagerContracts.Validation.Warehouse;

namespace WarehouseManager.Application.Services;

public class WarehouseService : ArchiveService<Warehouse, WarehouseSummary>, IWarehouseService
{
    private readonly IAuditService _auditService;

    public WarehouseService(AppDbContext context, IAuditService auditService) : base(context)
    {
        _auditService = auditService;
    }

    protected override async Task<WarehouseSummary> MapToSummaryAsync(Warehouse entity)
    {
        var activeStockCount = await _context.Remaining
            .CountAsync(r => r.WarehouseId == entity.Id && !r.IsArchived);

        var activeOrderCount = await _context.Orders
            .CountAsync(o => o.WarehouseId == entity.Id && !o.IsArchived);

        return new WarehouseSummary
        {
            Id = entity.Id,
            Address = entity.Address,
            Square = entity.Square,
            IsArchived = entity.IsArchived,
            CreationDatetime = entity.CreationDatetime,
            ActiveStockCount = activeStockCount,
            ActiveOrderCount = activeOrderCount
        };
    }

    protected override async Task<List<WarehouseSummary>> MapToSummariesAsync(List<Warehouse> entities)
    {
        var warehouseIds = entities.Select(w => w.Id).ToList();

        var stockCounts = await _context.Remaining
            .Where(r => warehouseIds.Contains(r.WarehouseId) && !r.IsArchived)
            .GroupBy(r => r.WarehouseId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        var orderCounts = await _context.Orders
            .Where(o => warehouseIds.Contains(o.WarehouseId) && !o.IsArchived)
            .GroupBy(o => o.WarehouseId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        return entities.Select(w => new WarehouseSummary
        {
            Id = w.Id,
            Address = w.Address,
            Square = w.Square,
            IsArchived = w.IsArchived,
            CreationDatetime = w.CreationDatetime,
            ActiveStockCount = stockCounts.GetValueOrDefault(w.Id, 0),
            ActiveOrderCount = orderCounts.GetValueOrDefault(w.Id, 0)
        }).ToList();
    }

    protected override IQueryable<Warehouse> ApplyCustomFilters(IQueryable<Warehouse> query, IPaginationFilter filter)
    {
        if (filter is not WarehouseFilter whFilter)
            return query;

        if (!string.IsNullOrWhiteSpace(whFilter.Address))
            query = query.Where(w => w.Address.Contains(whFilter.Address));

        return query;
    }

    public async Task<WarehouseSummary> CreateAsync(CreateWarehouseCommand command)
    {
        var validator = new CreateWarehouseCommandValidator();
        var result = await validator.ValidateAsync(command);
        if (!result.IsValid)
            throw new ModelValidationException(result.Errors.ToList());

        if (await _context.Warehouses.AnyAsync(x => x.Address == command.Address))
            throw new ConflictException("Адрес склада уже существует.");

        var warehouse = new Warehouse
        {
            Address = command.Address,
            Square = command.Square,
            IsArchived = false,
            CreationDatetime = DateTime.UtcNow
        };

        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync();

        _auditService.LogOperation(
            userId: command.UserId,
            action: $"Создание склада: {command.Address}",
            tableName: "Warehouses",
            recordId: warehouse.Id.ToString()
        );

        return await MapToSummaryAsync(warehouse);
    }

    public async Task<WarehouseSummary> UpdateAsync(UpdateWarehouseCommand command)
    {
        var validator = new UpdateWarehouseCommandValidator();
        var result = await validator.ValidateAsync(command);
        if (!result.IsValid)
            throw new ModelValidationException(result.Errors.ToList());

        var warehouse = await new EntityCheckHelper(_context).EnsureExistsAndActive<Warehouse>(command.Id);
        var trimmedAddress = command.Address.Trim();
        if (await _context.Warehouses.AnyAsync(x => x.Address.Trim() == trimmedAddress && x.Id != command.Id))
            throw new ConflictException("Адрес склада уже существует.");

        var oldAddress = warehouse.Address;
        warehouse.Address = command.Address;
        warehouse.Square = command.Square;
        warehouse.UpdateDatetime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _auditService.LogOperation(
            userId: command.UserId,
            action: "Обновление склада",
            oldValue: $"Address: {oldAddress}",
            newValue: $"Address: {command.Address}",
            tableName: "Warehouses",
            recordId: warehouse.Id.ToString()
        );

        return await MapToSummaryAsync(warehouse);
    }

    public async Task<bool> ArchiveAsync(int id, int userId)
    {
        var warehouse = await new EntityCheckHelper(_context).EnsureExistsAndActive<Warehouse>(id);

        // Проверяем существование пользователя перед логированием
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.IsArchived)
        {
            throw new DomainException("Пользователь не найден или деактивирован");
        }

        var hasActiveStock = await _context.Remaining
            .AnyAsync(r => r.WarehouseId == id && !r.IsArchived && r.Quantity > 0);

        var hasActiveOrders = await _context.Orders
            .AnyAsync(o => o.WarehouseId == id && !o.IsArchived && o.StatusId != 3); //На счет статуса здесь здесь еще нужно подумать!!!!!

        if (hasActiveStock || hasActiveOrders)
        {
            throw new ConflictException(
                "Нельзя архивировать склад: есть активные остатки или заказы");
        }

        warehouse.IsArchived = true;
        warehouse.UpdateDatetime = DateTime.UtcNow;

        _auditService.LogOperation(
            userId: userId,
            action: $"Архивирование склада: {warehouse.Address}",
            tableName: "Warehouses",
            recordId: warehouse.Id.ToString()
        );

        await _context.SaveChangesAsync();
        return true;
    }
}