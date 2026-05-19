using MSMES.Domain.Shared;

namespace MSMES.Domain.WorkOrder;

public class WorkOrder : Entity
{
    public string WorkOrderNo { get; set; } = string.Empty;
    public string? SalesOrderNo { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public decimal PlannedQuantity { get; set; }
    public decimal ProducedQuantity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Planned;

    public void Release()
    {
        if (Status != WorkOrderStatus.Planned)
            throw new InvalidOperationException("Only Planned WOs can be released.");
        Status = WorkOrderStatus.Released;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Start()
    {
        if (Status != WorkOrderStatus.Released)
            throw new InvalidOperationException("Only Released WOs can be started.");
        Status = WorkOrderStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete(decimal producedQty)
    {
        if (Status != WorkOrderStatus.InProgress)
            throw new InvalidOperationException("Only InProgress WOs can be completed.");
        ProducedQuantity = producedQty;
        ActualEndDate = DateTime.UtcNow;
        Status = WorkOrderStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Close()
    {
        Status = WorkOrderStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
    }
}
