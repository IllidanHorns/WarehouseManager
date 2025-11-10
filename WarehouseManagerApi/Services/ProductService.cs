/*using Microsoft.EntityFrameworkCore;
using WarehouseManagerApi.Data;
using WarehouseManagerApi.Models;
using WarehouseManagerApi.Services.Interface;
using WarehouseManagerContracts.DTOs.Product;
using WarehouseManagerContracts.DTOs.Remaining;
namespace WarehouseManagerApi.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync(bool includeArchived = false)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (!includeArchived)
                query = query.Where(p => !p.IsArchived);

            var products = await query.OrderBy(p => p.ProductName).ToListAsync();
            return products.Select(MapToDto);
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id && !p.IsArchived);

            return product == null ? null : MapToDto(product);
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId && !c.IsArchived);
            if (!categoryExists)
                throw new InvalidOperationException("Category does not exist or is archived.");

            var product = new Product
            {
                ProductName = dto.ProductName,
                Price = dto.Price,
                Weight = dto.Weight,
                CategoryId = dto.CategoryId,
                CreationDatetime = DateTime.UtcNow,
                UpdateDatetime = DateTime.UtcNow,
                IsArchived = false
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return MapToDto(product);
        }

        public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id && !p.IsArchived);
            if (product == null) return null;

            if (dto.CategoryId != product.CategoryId)
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId && !c.IsArchived);
                if (!categoryExists)
                    throw new InvalidOperationException("New category does not exist or is archived.");
            }

            bool priceChanged = product.Price != dto.Price;
            if (priceChanged)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.PriceHistories.Add(new PriceHistory
                    {
                        ProductId = product.ProductId,
                        OldPrice = product.Price,
                        NewPrice = dto.Price,
                        CreationDatetime = DateTime.UtcNow,
                        UpdateDatetime = DateTime.UtcNow,
                        IsArchived = false
                    });

                    product.ProductName = dto.ProductName;
                    product.Price = dto.Price;
                    product.Weight = dto.Weight;
                    product.CategoryId = dto.CategoryId;
                    product.UpdateDatetime = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            else
            {
                product.ProductName = dto.ProductName;
                product.Weight = dto.Weight;
                product.CategoryId = dto.CategoryId;
                product.UpdateDatetime = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return MapToDto(product);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id && !p.IsArchived);
            if (product == null) return false;

            var isInActiveOrder = await _context.OrdersProducts
                .AnyAsync(op => op.ProductId == id && !op.Order.IsArchived && !op.IsArchived);
            if (isInActiveOrder)
                throw new InvalidOperationException("Cannot delete product used in active orders.");

            var hasRemaining = await _context.Remaining
                .AnyAsync(r => r.ProductId == id && !r.IsArchived);
            if (hasRemaining)
                throw new InvalidOperationException("Cannot delete product with existing stock.");

            product.IsArchived = true;
            product.UpdateDatetime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        private ProductDto MapToDto(Product product) => new()
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            Price = product.Price,
            Weight = product.Weight,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            CreationDatetime = product.CreationDatetime
        };


        public async Task<IEnumerable<RemainingDto>> GetStockAsync(int productId)
        {
            var stock = await _context.Remaining
                .Include(r => r.Product)
                .Include(r => r.Warehouse)
                .Where(r => r.ProductId == productId && !r.IsArchived)
                .OrderBy(r => r.Warehouse.Address)
                .ToListAsync();

            return stock.Select(r => new RemainingDto
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
    }
}
*/