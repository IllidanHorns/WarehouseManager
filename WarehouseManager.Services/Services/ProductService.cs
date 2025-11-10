using FluentValidation;
using Microsoft.EntityFrameworkCore;
using WarehouseManager.Core.Data;
using WarehouseManager.Core.Models;
using WarehouseManager.Services.Exceptions;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Filters.Interfaces;
using WarehouseManager.Services.Helpers;
using WarehouseManager.Services.Services.Base;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManagerContracts.DTOs.Product;
using WarehouseManagerContracts.Validation.Product;

namespace WarehouseManager.Application.Services;

public class ProductService : ArchiveService<Product, ProductSummary>, IProductService
{
    private readonly IAuditService _auditService;
    private readonly ITransactionManager _transactionManager;

    public ProductService(AppDbContext context, IAuditService auditService, ITransactionManager transactionManager) : base(context)
    {
        _auditService = auditService;
        _transactionManager = transactionManager;
    }

    protected override async Task<ProductSummary> MapToSummaryAsync(Product entity)
    {
        var category = await _context.Categories.FindAsync(entity.CategoryId);
        return new ProductSummary
        {
            Id = entity.Id,
            Name = entity.ProductName,
            Price = entity.Price,
            Weight = entity.Weight,
            CategoryId = entity.CategoryId,
            CategoryName = category?.Name ?? "Без категории",
            IsArchived = entity.IsArchived,
            CreationDatetime = entity.CreationDatetime
        };
    }

    protected override async Task<List<ProductSummary>> MapToSummariesAsync(List<Product> entities)
    {
        var categoryIds = entities.Select(e => e.CategoryId).Distinct().ToList();
        var categories = await _context.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id);

        return entities.Select(e => new ProductSummary
        {
            Id = e.Id,
            Name = e.ProductName,
            Price = e.Price,
            Weight = e.Weight,
            CategoryName = categories.GetValueOrDefault(e.CategoryId)?.Name ?? "Без категории",
            IsArchived = e.IsArchived,
            CreationDatetime = e.CreationDatetime
        }).ToList();
    }

    protected override IQueryable<Product> ApplyCustomFilters(IQueryable<Product> query, IPaginationFilter filter)
    {
        if (filter is not ProductsFilters productFilter)
            return query;

        if (!string.IsNullOrWhiteSpace(productFilter.Name))
            query = query.Where(p => p.ProductName.Contains(productFilter.Name));

        if (productFilter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == productFilter.CategoryId.Value);

        if (productFilter.WarehouseId.HasValue)
        {
            var warehouseId = productFilter.WarehouseId.Value;
            // Фильтруем товары, которые имеют остатки на указанном складе
            // Используем прямой запрос к таблице остатков для надежности
            var productIdsWithStock = _context.Remaining
                .Where(r => r.WarehouseId == warehouseId && !r.IsArchived)
                .Select(r => r.ProductId)
                .Distinct();
            
            query = query.Where(p => productIdsWithStock.Contains(p.Id));
        }

        return query;
    }

    public async Task<ProductSummary> CreateAsync(CreateProductCommand command)
    {
        var validator = new CreateProductDtoValidator();
        var result = await validator.ValidateAsync(command);
        if (!result.IsValid)
            throw new ModelValidationException(result.Errors.ToList());

        if (await _context.Products.AnyAsync(x => x.ProductName == command.ProductName))
            throw new ConflictException("Название продукта уже существует.");

        await new EntityCheckHelper(_context).EnsureExistsAndActive<Category>(command.CategoryId);

        var product = new Product
        {
            ProductName = command.ProductName,
            Price = command.Price,
            Weight = command.Weight,
            CategoryId = command.CategoryId,
            IsArchived = false,
            CreationDatetime = DateTime.UtcNow,
            UpdateDatetime = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        _auditService.LogOperation(
            userId: command.UserId,
            action: $"Создание продукта: {product.ProductName}",
            tableName: "Products",
            recordId: product.Id.ToString()
        );

        await _context.SaveChangesAsync();

        return await MapToSummaryAsync(product);
    }

    public async Task<ProductSummary> UpdateAsync(UpdateProductCommand command)
    {
        return await _transactionManager.ExecuteCatalogWorkflowAsync(async () =>
        {
            var validator = new UpdateProductDtoValidator();
            var result = await validator.ValidateAsync(command);
            if (!result.IsValid)
                throw new ModelValidationException(result.Errors.ToList());

            if (await _context.Products.AnyAsync(x => x.ProductName == command.ProductName && x.Id != command.Id))
                throw new ConflictException("Название продукта уже существует.");

            var product = await new EntityCheckHelper(_context).EnsureExistsAndActive<Product>(command.Id);
            var oldProductName = product.ProductName;
            var oldPrice = product.Price;

            await new EntityCheckHelper(_context).EnsureExistsAndActive<Category>(command.CategoryId);

            product.ProductName = command.ProductName;
            product.Price = command.Price;
            product.Weight = command.Weight;
            product.CategoryId = command.CategoryId;
            product.UpdateDatetime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _auditService.LogOperation(
                userId: command.UserId,
                action: $"Обновление продукта ID={product.Id}",
                oldValue: $"Название: {oldProductName}, Цена: {oldPrice}",
                newValue: $"Название: {product.ProductName}, Цена: {product.Price}",
                tableName: "Products",
                recordId: product.Id.ToString()
            );

            await _context.SaveChangesAsync();

            return await MapToSummaryAsync(product);
        });
    }

    public async Task<bool> ArchiveAsync(int productId, int userId)
    {
        var product = await new EntityCheckHelper(_context).EnsureExistsAndActive<Product>(productId);

        var hasActiveRemainings = await _context.Remaining
            .AnyAsync(r => r.ProductId == productId && !r.IsArchived && r.Quantity > 0);
        if (hasActiveRemainings)
            throw new ConflictException("Нельзя архивировать продукт: есть активные остатки на складах.");

        var hasActiveOrders = await _context.OrdersProducts
            .Include(op => op.Order)
            .AnyAsync(op => op.ProductId == productId &&
                            !op.IsArchived &&
                            !op.Order.IsArchived &&
                            op.Order.StatusId != 3); // 3 = "Отменён"
        if (hasActiveOrders)
            throw new ConflictException("Нельзя архивировать продукт: он используется в активных заказах.");

        // Проверяем существование пользователя перед логированием
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.IsArchived)
        {
            throw new DomainException("Пользователь не найден или деактивирован");
        }

        product.IsArchived = true;
        product.UpdateDatetime = DateTime.UtcNow;

        _auditService.LogOperation(
            userId: userId,
            action: $"Архивирование продукта: {product.ProductName}",
            tableName: "Products",
            recordId: product.Id.ToString()
        );

        await _context.SaveChangesAsync();
        return true;
    }
}