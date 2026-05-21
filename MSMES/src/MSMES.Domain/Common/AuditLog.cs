namespace MSMES.Domain.Common;

public class AuditLog
{
    public long   Id          { get; set; }
    public string UserId      { get; set; } = string.Empty;
    public string UserName    { get; set; } = string.Empty;
    public string Action      { get; set; } = string.Empty;
    public string EntityType  { get; set; } = string.Empty;
    public string? EntityId   { get; set; }
    public string? Description { get; set; }
    public string? IpAddress  { get; set; }
    public DateTime CreatedAt { get; set; }
}

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken ct = default);
    Task<IReadOnlyList<AuditLog>> ListAsync(int skip, int take, CancellationToken ct = default);
    Task<IReadOnlyList<AuditLog>> ListByUserAsync(string userId, int take, CancellationToken ct = default);
    Task<IReadOnlyList<AuditLog>> ListByEntityAsync(string entityType, string entityId, CancellationToken ct = default);
}
