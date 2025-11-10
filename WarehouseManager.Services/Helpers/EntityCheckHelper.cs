using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManager.Core.Data;
using WarehouseManager.Services.Exceptions;
using WarehouseManger.Core.Models.Interfaces;

namespace WarehouseManager.Services.Helpers
{
    public class EntityCheckHelper 
    {
        private readonly AppDbContext _context;

        public EntityCheckHelper(AppDbContext dbContext) 
        {
            _context = dbContext;
        }

        public async Task<T> EnsureExistsAndActive<T>(int id, string? entityName = null) 
            where T : class, IEntity, IArchivable
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity == null || entity.IsArchived) 
            {
                if (entityName == null)
                {
                    throw new DomainException("Не найдено или не доступно");
                }

                throw new DomainException($"{entityName} - не найдено или не доступно");
            }

            return entity;
        }

        public async Task<T> EnsureExists<T>(int id, string? entityName = null)
            where T : class, IEntity
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity == null) 
            {
                if (entityName == null)
                {
                    throw new DomainException("Не найдено или не доступно");
                }

                throw new DomainException($"{entityName} - не найдено или не доступно");
            }

            return entity;
        }
    }
}
