using System.Threading.Tasks;
using WarehouseManager.Core.Data;
using WarehouseManager.Services.Helpers;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary.Interfaces;
using WarehouseManger.Core.Models.Interfaces;

namespace WarehouseManager.Services.Services.Base
{
    public abstract class ArchiveService<T, TSummary> : ReadOnlyArchivableService<T, TSummary>, IArchiveService
        where T : class, IEntity, IArchivable
        where TSummary : class, ISummary
    {

        protected ArchiveService(AppDbContext context) : base(context) { }

        public virtual async Task<bool> ArchiveAsync(int id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity == null)
                return false;

            if (entity.IsArchived)
                return true; 

            entity.IsArchived = true;
            _context.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public virtual async Task<bool> RestoreAsync(int id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity == null)
                return false; 

            if (!entity.IsArchived)
                return true; 

            entity.IsArchived = false;
            _context.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}