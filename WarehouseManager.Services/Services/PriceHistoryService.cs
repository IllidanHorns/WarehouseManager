using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WarehouseManager.Core.Data;
using WarehouseManager.Core.Models;
using WarehouseManager.Services;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Filters.Interfaces;
using WarehouseManager.Services.Services.Base;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary;

namespace WarehouseManager.Application.Services
{
    public class PriceHistoryService : ReadOnlyArchivableService<PriceHistory, PriceHistorySummary>, IPriceHistoryService
    {
        public PriceHistoryService(AppDbContext context) : base(context)
        {
        }

        protected override IQueryable<PriceHistory> ApplyCustomFilters(IQueryable<PriceHistory> query, IPaginationFilter filter)
        {
            query = query.Include(ph => ph.Product)
                         .ThenInclude(p => p.Category);

            if (filter is PriceHistoryFilter priceHistoryFilter)
            {
                if (!string.IsNullOrWhiteSpace(priceHistoryFilter.ProductName))
                {
                    var name = priceHistoryFilter.ProductName.Trim();
                    query = query.Where(ph => ph.Product.ProductName.Contains(name));
                }

                if (priceHistoryFilter.CategoryId.HasValue)
                {
                    var categoryId = priceHistoryFilter.CategoryId.Value;
                    query = query.Where(ph => ph.Product.CategoryId == categoryId);
                }
            }

            return query;
        }

        public override async Task<PagedResult<PriceHistorySummary>> GetPagedAsync(IPaginationFilter filter)
        {
            IQueryable<PriceHistory> query = _context.PriceHistories
                .Include(ph => ph.Product)
                    .ThenInclude(p => p.Category);

            if (!filter.IncludeArchived)
            {
                query = query.Where(ph => !ph.IsArchived);
            }

            query = ApplyCustomFilters(query, filter);

            var totalCount = await query.CountAsync();

            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize <= 0 ? 20 : filter.PageSize;

            var items = await query
                .OrderByDescending(ph => ph.UpdateDatetime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var summaries = await MapToSummariesAsync(items);

            return new PagedResult<PriceHistorySummary>
            {
                Items = summaries,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        protected override async Task<PriceHistorySummary> MapToSummaryAsync(PriceHistory entity)
        {
            if (entity.Product == null)
            {
                entity = await _context.PriceHistories
                    .Include(ph => ph.Product)
                        .ThenInclude(p => p.Category)
                    .FirstAsync(ph => ph.Id == entity.Id);
            }

            return new PriceHistorySummary
            {
                Id = entity.Id,
                ProductName = entity.Product?.ProductName ?? "Неизвестный продукт",
                CategoryName = entity.Product?.Category?.Name ?? "Без категории",
                OldPrice = entity.OldPrice,
                NewPrice = entity.NewPrice,
                UpdateDatetime = entity.UpdateDatetime
            };
        }

        protected override Task<List<PriceHistorySummary>> MapToSummariesAsync(List<PriceHistory> entities)
        {
            var summaries = entities.Select(entity => new PriceHistorySummary
            {
                Id = entity.Id,
                ProductName = entity.Product?.ProductName ?? "Неизвестный продукт",
                CategoryName = entity.Product?.Category?.Name ?? "Без категории",
                OldPrice = entity.OldPrice,
                NewPrice = entity.NewPrice,
                UpdateDatetime = entity.UpdateDatetime
            }).ToList();

            return Task.FromResult(summaries);
        }
    }
}
