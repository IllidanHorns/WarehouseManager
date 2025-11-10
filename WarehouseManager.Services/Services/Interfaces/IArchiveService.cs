using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManager.Services.Services.Interfaces
{
    public interface IArchiveService
    {
        Task<bool> ArchiveAsync(int id);
        Task<bool> RestoreAsync(int id);
    }
}
