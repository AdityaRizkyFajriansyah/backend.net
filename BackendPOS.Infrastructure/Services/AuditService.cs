using BackendPOS.Application.Services;
using BackendPOS.Domain.Entities;
using BackendPOS.Infrastructure.Data;

namespace BackendPOS.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly PosDbContext _db;
    public AuditService(PosDbContext db) => _db = db;

    public async Task WriteAsync(AuditLog log)
    {
        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}