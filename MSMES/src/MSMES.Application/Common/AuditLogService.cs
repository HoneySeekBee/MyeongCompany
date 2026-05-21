using MSMES.Domain.Common;

namespace MSMES.Application.Common;

public class AuditLogService
{
    private readonly IAuditLogRepository _repo;
    public AuditLogService(IAuditLogRepository repo) => _repo = repo;

    public Task LogAsync(string userId, string userName, string action,
        string entityType, string? entityId = null, string? description = null,
        string? ipAddress = null, CancellationToken ct = default)
        => _repo.AddAsync(new AuditLog
        {
            UserId      = userId,
            UserName    = userName,
            Action      = action,
            EntityType  = entityType,
            EntityId    = entityId,
            Description = description,
            IpAddress   = ipAddress,
            CreatedAt   = DateTime.UtcNow
        }, ct);
}
