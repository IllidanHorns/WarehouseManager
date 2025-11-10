using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManager.Core.Data;
using WarehouseManager.Services.Filters.Interfaces;
using WarehouseManager.Services.Helpers;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary.Interfaces;
using WarehouseManger.Core.Models.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WarehouseManager.Services.Services.Base
{
    public abstract class ReadOnlyService<T, TSummary> : IReadService<TSummary>
    where T : class, IEntity
    where TSummary : class, ISummary
    {
        protected readonly AppDbContext _context;
        protected  readonly EntityCheckHelper _entityCheckHelper;

        protected ReadOnlyService(AppDbContext context)
        {
            _context = context;
            _entityCheckHelper = new EntityCheckHelper(context);
        }

        protected virtual IQueryable<T> ApplyCustomFilters(IQueryable<T> query, IPaginationFilter filter)
        {
            return query;
        }

        public virtual async Task<TSummary> GetByIdAsync(int id)
        {
            var entity = await _entityCheckHelper.EnsureExists<T>(id);
            return await MapToSummaryAsync(entity);
        }

        public virtual async Task<PagedResult<TSummary>> GetPagedAsync(IPaginationFilter filter)
        {
            IQueryable<T> query = _context.Set<T>();

            var totalCount = await query.CountAsync();

            query = ApplyCustomFilters(query, filter);

            var pagedItems = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var summaries = await MapToSummariesAsync(pagedItems);

            return new PagedResult<TSummary>
            {
                Items = summaries,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        protected abstract Task<TSummary> MapToSummaryAsync(T entity);
        protected abstract Task<List<TSummary>> MapToSummariesAsync(List<T> entities);
    }
}
