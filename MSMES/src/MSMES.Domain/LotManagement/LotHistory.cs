using MSMES.Domain.Shared;

namespace MSMES.Domain.LotManagement;

public class LotHistory : Entity
{
    public long HistoryId { get; set; }
    public string LotNo { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public DateTime OperatedAt { get; set; }
    public string Operator { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}
