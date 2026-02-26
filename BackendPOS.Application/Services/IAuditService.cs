using BackendPOS.Domain.Entities;

namespace BackendPOS.Application.Services;

public interface IAuditService
{
    Task WriteAsync(AuditLog log);
}