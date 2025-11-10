/*// Services/Implementations/WarehouseService.cs
using Microsoft.EntityFrameworkCore;
using WarehouseManager.Core.Data;
using WarehouseManagerApi.Services.Interface;
using WarehouseManagerContracts.DTOs.Remaining;
using WarehouseManagerContracts.DTOs.Warehouse;

namespace WarehouseManagerApi.Services.Implementations;

public class WarehouseService : IWarehouseService
{
    private readonly AppDbContext _context;

    public WarehouseService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WarehouseDto>> GetAllAsync(bool includeArchived = false)
    {
        var query = _context.Warehouses.AsQueryable();
        if (!includeArchived)
            query = query.Where(w => !w.IsArchived);

        var warehouses = await query.OrderBy(w => w.Address).ToListAsync();
        return warehouses.Select(MapToDto);
    }

    public async Task<WarehouseDto?> GetByIdAsync(int id)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.WarehouseId == id && !w.IsArchived);
        return warehouse == null ? null : MapToDto(warehouse);
    }

    public async Task<WarehouseDto> CreateAsync(CreateWarehouseCommand dto)
    {
        var warehouse = new Warehouse
        {
            Address = dto.Address,
            Square = dto.Square,
            CreationDatetime = DateTime.UtcNow,
            UpdateDatetime = DateTime.UtcNow,
            IsArchived = false
        };

        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync();

        return MapToDto(warehouse);
    }

    public async Task<WarehouseDto?> UpdateAsync(int id, UpdateWarehouseCommand dto)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.WarehouseId == id && !w.IsArchived);
        if (warehouse == null) return null;

        warehouse.Address = dto.Address;
        warehouse.Square = dto.Square;
        warehouse.UpdateDatetime = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(warehouse);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.WarehouseId == id && !w.IsArchived);
        if (warehouse == null) return false;

        var hasRemaining = await _context.Remaining
            .AnyAsync(r => r.WarehouseId == id && !r.IsArchived);
        if (hasRemaining)
            throw new InvalidOperationException("Cannot delete warehouse with existing stock.");

        var hasOrders = await _context.Orders
            .AnyAsync(o => o.WarehouseId == id && !o.IsArchived);
        if (hasOrders)
            throw new InvalidOperationException("Cannot delete warehouse with active orders.");

        warehouse.IsArchived = true;
        warehouse.UpdateDatetime = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<RemainingDto>> GetRemainingAsync(int warehouseId)
    {
        var remaining = await _context.Remaining
            .Include(r => r.Product)
            .Include(r => r.Warehouse)
            .Where(r => r.WarehouseId == warehouseId && !r.IsArchived)
            .OrderBy(r => r.Product.ProductName)
            .ToListAsync();

        return remaining.Select(r => new RemainingDto
        {
            RemainingId = r.RemainingId,
            Quantity = r.Quantity,
            ProductId = r.ProductId,
            ProductName = r.Product.ProductName,
            WarehouseId = r.WarehouseId,
            WarehouseAddress = r.Warehouse.Address,
            CreationDatetime = r.CreationDatetime
        });
    }

    private static WarehouseDto MapToDto(Warehouse warehouse) => new()
    {
        WarehouseId = warehouse.WarehouseId,
        Address = warehouse.Address,
        Square = warehouse.Square,
        CreationDatetime = warehouse.CreationDatetime
    };
}*/