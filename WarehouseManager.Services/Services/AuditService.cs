using WarehouseManager.Core.Data;
using WarehouseManager.Core.Models;
using WarehouseManager.Services.Services.Interfaces;

namespace WarehouseManager.Application.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;

    public AuditService(AppDbContext context)
    {
        _context = context;
    }

    public void LogOperation(
        int userId,
        string action,
        string? oldValue = null,
        string? newValue = null,
        string? tableName = null,
        string? recordId = null)
    {
        var auditRecord = new OperationsAudit
        {
            UserId = userId,
            OperationType = action ?? string.Empty,
            OldData = oldValue ?? string.Empty,
            NewData = newValue ?? string.Empty,
            TableName = tableName ?? string.Empty,
            RecordId = recordId ?? string.Empty,
            OperationTime = DateTime.UtcNow
        };

        _context.OperationsAudits.Add(auditRecord);
    }
}