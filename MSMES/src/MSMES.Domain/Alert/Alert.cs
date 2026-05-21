namespace MSMES.Domain.Alert;

public class Alert
{
    public int     Id         { get; set; }
    public string  AlertType  { get; set; } = string.Empty;
    public byte    Severity   { get; set; } = 1;
    public string  Title      { get; set; } = string.Empty;
    public string  Message    { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityId   { get; set; }
    public bool    IsRead     { get; set; }
    public bool    IsResolved { get; set; }
    public DateTime CreatedAt { get; set; }

    public string SeverityName => Severity switch { 3 => "위험", 2 => "경고", _ => "정보" };
    public string SeverityCss  => Severity switch { 3 => "danger", 2 => "warning", _ => "info" };
    public string AlertTypeIcon => AlertType switch {
        "STOCK_LOW"    => "bi-box-seam",
        "EQUIP_FAULT"  => "bi-tools",
        "DELIVERY_DUE" => "bi-clock",
        "QUALITY"      => "bi-patch-exclamation",
        _ => "bi-info-circle"
    };
}

public interface IAlertRepository
{
    Task<IReadOnlyList<Alert>> ListAsync(bool includeResolved, CancellationToken ct = default);
    Task<int>                  CountUnreadAsync(CancellationToken ct = default);
    Task                       MarkReadAsync(int id, CancellationToken ct = default);
    Task                       ResolveAsync(int id, CancellationToken ct = default);
}
