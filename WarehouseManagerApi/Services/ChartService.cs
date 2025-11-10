/*using WarehouseManager.Contracts.DTOs.ViewsDiagrams;
using WarehouseManagerApi.Data;
using WarehouseManagerApi.Services.Interface;
using WarehouseManagerContracts.DTOs.OrderStatus;
using Microsoft.EntityFrameworkCore;

namespace WarehouseManagerApi.Services
{
    public class ChartService : IChartService
    {
        private readonly AppDbContext _context;

        public ChartService(AppDbContext context)
        {
            _context = context;
        }

        public Task<List<WarehouseStockDto>> GetWarehouseStockAsync()
            => _context.Set<WarehouseStockDto>().ToListAsync();

        public Task<List<OrderStatusDto>> GetOrderStatusAsync()
            => _context.Set<OrderStatusDto>().ToListAsync();

        public Task<List<CategoryProductCountDto>> GetCategoryProductCountAsync()
            => _context.Set<CategoryProductCountDto>().ToListAsync();
    }
}
*/