/*// Services/Implementations/CategoryService.cs
using Microsoft.EntityFrameworkCore;
using WarehouseManagerApi.Data;
using WarehouseManagerApi.Models;
using WarehouseManagerApi.Services.Interface;
using WarehouseManagerApi.Services.Interface;
using WarehouseManagerContracts.DTOs.Category;

namespace WarehouseManagerApi.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync(bool includeArchived = false)
    {
        var query = _context.Categories.AsQueryable();

        if (!includeArchived)
            query = query.Where(c => !c.IsArchived);

        var categories = await query
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories.Select(MapToDto);
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == id && !c.IsArchived);

        return category == null ? null : MapToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryCommand dto)
    {
        var exists = await _context.Categories
            .AnyAsync(c => c.Name == dto.Name && !c.IsArchived);
        if (exists)
            throw new InvalidOperationException($"Category with name '{dto.Name}' already exists.");

        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            CreationDatetime = DateTime.UtcNow,
            UpdateDatetime = DateTime.UtcNow,
            IsArchived = false
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == id && !c.IsArchived);
        if (category == null) return null;

        var nameExists = await _context.Categories
            .AnyAsync(c => c.Name == dto.Name && c.CategoryId != id && !c.IsArchived);
        if (nameExists)
            throw new InvalidOperationException($"Category with name '{dto.Name}' already exists.");

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.UpdateDatetime = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(category);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == id && !c.IsArchived);
        if (category == null) return false;

        var hasActiveProducts = await _context.Products
            .AnyAsync(p => p.CategoryId == id && !p.IsArchived);
        if (hasActiveProducts)
            throw new InvalidOperationException("Cannot delete category with active products.");

        category.IsArchived = true;
        category.UpdateDatetime = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    private static CategoryDto MapToDto(Category category) => new()
    {
        CategoryId = category.CategoryId,
        Name = category.Name,
        Description = category.Description,
        CreationDatetime = category.CreationDatetime
    };
}*/