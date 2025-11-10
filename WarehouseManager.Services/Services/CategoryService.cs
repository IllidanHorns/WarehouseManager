using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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
using WarehouseManagerContracts.DTOs.Category;
using WarehouseManagerContracts.Validation.Category;

namespace WarehouseManager.Application.Services;

public class CategoryService : ArchiveService<Category, CategorySummary>, ICategoryService
{

    public CategoryService(AppDbContext context) : base(context)
    {
    }

    protected override async Task<CategorySummary> MapToSummaryAsync(Category entity)
    {
        var activeProductCount = await _context.Products
            .CountAsync(p => p.CategoryId == entity.Id && !p.IsArchived);

        return new CategorySummary
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsArchived = entity.IsArchived,
            CreationDatetime = entity.CreationDatetime,
            ActiveProductCount = activeProductCount
        };
    }

    protected override async Task<List<CategorySummary>> MapToSummariesAsync(List<Category> entities)
    {
        var categoryIds = entities.Select(c => c.Id).ToList();
        var productCounts = await _context.Products
            .Where(p => categoryIds.Contains(p.CategoryId) && !p.IsArchived)
            .GroupBy(p => p.CategoryId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        return entities.Select(c => new CategorySummary
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            IsArchived = c.IsArchived,
            CreationDatetime = c.CreationDatetime,
            ActiveProductCount = productCounts.GetValueOrDefault(c.Id, 0)
        }).ToList();
    }

    protected override IQueryable<Category> ApplyCustomFilters(IQueryable<Category> query, IPaginationFilter filter)
    {
        if (filter is not CategoryFilter catFilter)
            return query;

        if (!string.IsNullOrWhiteSpace(catFilter.Name))
            query = query.Where(c => c.Name.Contains(catFilter.Name));

        return query;
    }

    public async Task<CategorySummary> CreateAsync(CreateCategoryCommand command)
    {
        var validator = new CreateCategoryCommandValidator();
        var result = await validator.ValidateAsync(command);
        if (!result.IsValid)
            throw new ModelValidationException(result.Errors.ToList());

        if (await _context.Categories.AnyAsync(x => x.Name == command.Name))
            throw new ConflictException("Название категории уже существует.");

        var category = new Category
        {
            Name = command.Name,
            Description = command.Description,
            IsArchived = false,
            CreationDatetime = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return await MapToSummaryAsync(category);
    }

    public async Task<CategorySummary> UpdateAsync(UpdateCategoryCommand command)
    {
        var validator = new UpdateCategoryCommandValidator();
        var result = await validator.ValidateAsync(command);
        if (!result.IsValid)
            throw new ModelValidationException(result.Errors.ToList());

        var category = await new EntityCheckHelper(_context).EnsureExistsAndActive<Category>(command.Id);

        if (await _context.Categories.AnyAsync(x => x.Name == command.Name && x.Id != command.Id))
            throw new ConflictException("Название категории уже существует.");

        var oldName = category.Name;
        category.Name = command.Name;
        category.Description = command.Description;
        category.UpdateDatetime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await MapToSummaryAsync(category);
    }

    public async Task<bool> ArchiveAsync(int id, int userId)
    {
        var category = await new EntityCheckHelper(_context).EnsureExistsAndActive<Category>(id);

        var hasActiveProducts = await _context.Products
            .AnyAsync(p => p.CategoryId == id && !p.IsArchived);

        if (hasActiveProducts)
            throw new ConflictException("Нельзя архивировать категорию: есть активные товары");

        category.IsArchived = true;
        category.UpdateDatetime = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}