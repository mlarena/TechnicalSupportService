using TechnicalSupportService.Core.Interfaces;
using TechnicalSupportService.Data.Context;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.SUTP.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _db;
    public AuditService(ApplicationDbContext db) => _db = db;

    public async Task LogAsync(string action, Guid userId, string? entityName = null,
        Guid? entityId = null, string? details = null, string? ipAddress = null, string? userAgent = null)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId, Action = action, EntityName = entityName,
            EntityId = entityId, Details = details, IpAddress = ipAddress,
            UserAgent = userAgent, CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }
}
