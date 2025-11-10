using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManager.Services.Services.Interfaces
{
    public interface IAuditService
    {
        void LogOperation(
            int userId,
            string action,
            string? oldValue = null,
            string? newValue = null,
            string? tableName = null,
            string? recordId = null
            );
    }
}
