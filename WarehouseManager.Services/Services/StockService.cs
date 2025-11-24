using Microsoft.EntityFrameworkCore;
using WarehouseManager.Contracts.DTOs.Remaining;
using WarehouseManager.Contracts.Validation.Remaining;
using WarehouseManager.Core.Data;
using WarehouseManager.Core.Models;
using WarehouseManager.Services.Exceptions;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Filters.Interfaces;
using WarehouseManager.Services.Helpers;
using WarehouseManager.Services.Services.Base;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary;

namespace WarehouseManager.Application.Services;

public class StockService : ReadOnlyService<Remaining, WarehouseStockSummary>, IStockService
{

    public StockService(AppDbContext context) : base(context)
    {
    }

    protected override async Task<WarehouseStockSummary> MapToSummaryAsync(Remaining entity)
    {
        var product = await _context.Products.FindAsync(entity.ProductId);
        var warehouse = await _context.Warehouses.FindAsync(entity.WarehouseId);
        return new WarehouseStockSummary
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            ProductName = product?.ProductName ?? "Удалён",
            WarehouseId = entity.WarehouseId,
            WarehouseAddress = warehouse?.Address ?? "Удалён",
            Quantity = entity.Quantity,
            ProductIsArchived = product?.IsArchived ?? true,
            WarehouseIsArchived = warehouse?.IsArchived ?? true
        };
    }

    protected override async Task<List<WarehouseStockSummary>> MapToSummariesAsync(List<Remaining> entities)
    {
        var productIds = entities.Select(e => e.ProductId).ToList();
        var warehouseIds = entities.Select(e => e.WarehouseId).ToList();

        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var warehouses = await _context.Warehouses
            .Where(w => warehouseIds.Contains(w.Id))
            .ToDictionaryAsync(w => w.Id);

        return entities.Select(e => new WarehouseStockSummary
        {
            Id = e.Id,
            ProductId = e.ProductId,
            ProductName = products.GetValueOrDefault(e.ProductId)?.ProductName ?? "Удалён",
            WarehouseId = e.WarehouseId,
            WarehouseAddress = warehouses.GetValueOrDefault(e.WarehouseId)?.Address ?? "Удалён",
            Quantity = e.Quantity,
            ProductIsArchived = products.GetValueOrDefault(e.ProductId)?.IsArchived ?? true,
            WarehouseIsArchived = warehouses.GetValueOrDefault(e.WarehouseId)?.IsArchived ?? true
        }).ToList();
    }

    protected override IQueryable<Remaining> ApplyCustomFilters(IQueryable<Remaining> query, IPaginationFilter filter)
    {
        if (filter is not StockFilter stockFilter)
            return query;

        if (!stockFilter.IncludeArchived)
        {
            query = query.Where(r => !r.IsArchived);
        }
            
        if (stockFilter.ProductId.HasValue)
            query = query.Where(r => r.ProductId == stockFilter.ProductId.Value);

        if (stockFilter.WarehouseId.HasValue)
            query = query.Where(r => r.WarehouseId == stockFilter.WarehouseId.Value);

        if (!string.IsNullOrWhiteSpace(stockFilter.ProductName))
        {
            query = query.Include(r => r.Product)
                .Where(r => r.Product.ProductName.Contains(stockFilter.ProductName));
        }

        if (!string.IsNullOrWhiteSpace(stockFilter.WarehouseAddress))
        {
            query = query.Include(r => r.Warehouse)
                .Where(r => r.Warehouse.Address.Contains(stockFilter.WarehouseAddress));
        }

        return query;
    }

    public async Task<WarehouseStockSummary> CreateAsync(CreateStockCommand command)
    {
        var validator = new CreateStockCommandValidator();
        var result = await validator.ValidateAsync(command);
        if (!result.IsValid)
            throw new ModelValidationException(result.Errors.ToList());

        await new EntityCheckHelper(_context).EnsureExistsAndActive<Product>(command.ProductId);

        // Проверяем существование склада
        await new EntityCheckHelper(_context).EnsureExistsAndActive<Warehouse>(command.WarehouseId);

        // Проверяем, что остаток для этого продукта и склада еще не существует
        var existingRemaining = await _context.Remaining
            .FirstOrDefaultAsync(r => r.ProductId == command.ProductId && 
                                     r.WarehouseId == command.WarehouseId && 
                                     !r.IsArchived);

        if (existingRemaining != null)
            throw new ConflictException($"Остаток для продукта на этом складе уже существует. Используйте обновление для изменения количества.");

        var remaining = new Remaining
        {
            ProductId = command.ProductId,
            WarehouseId = command.WarehouseId,
            Quantity = command.Quantity,
            IsArchived = false,
            CreationDatetime = DateTime.UtcNow,
            UpdateDatetime = DateTime.UtcNow
        };

        _context.Remaining.Add(remaining);
        await _context.SaveChangesAsync();

        return await MapToSummaryAsync(remaining);
    }

    public new async Task<WarehouseStockSummary> GetByIdAsync(int remainingId)
    {
        var remaining = await _context.Remaining
            .FirstOrDefaultAsync(r => r.Id == remainingId);

        if (remaining == null || remaining.IsArchived)
            throw new DomainException($"Остаток с ID {remainingId} не найден или архивирован");

        return await MapToSummaryAsync(remaining);
    }

    public async Task<WarehouseStockSummary> UpdateStockAsync(UpdateStockCommand command)
    {
        var validator = new UpdateStockCommandValidator();
        var result = await validator.ValidateAsync(command);
        if (!result.IsValid)
            throw new ModelValidationException(result.Errors.ToList());

        var remaining = await _context.Remaining
            .Include(r => r.Product)
            .Include(r => r.Warehouse)
            .FirstOrDefaultAsync(r => r.Id == command.RemainingId && !r.IsArchived);

        if (remaining == null)
            throw new DomainException("Остаток не найден");

        // Проверяем, что новое количество не отрицательное (валидатор уже проверяет, но лучше перестраховаться)
        if (command.NewQuantity < 0)
            throw new DomainException("Количество не может быть отрицательным");

        var oldQuantity = remaining.Quantity;
        remaining.Quantity = command.NewQuantity;
        remaining.UpdateDatetime = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await MapToSummaryAsync(remaining);
    }
}