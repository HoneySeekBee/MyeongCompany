using MSMES.Domain.Shared;

namespace MSMES.Domain.Process;

public class ProductionResult : Entity
{
    public string ResultNo { get; set; } = string.Empty;
    public string WorkOrderNo { get; set; } = string.Empty;
    public string ProcessCode { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public decimal ProducedQuantity { get; set; }
    public decimal DefectQuantity { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public decimal GoodQuantity => ProducedQuantity - DefectQuantity;
    public decimal? DurationMinutes =>
        EndTime.HasValue ? (decimal)(EndTime.Value - StartTime).TotalMinutes : null;
}
