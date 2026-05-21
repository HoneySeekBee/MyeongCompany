using MSMES.Domain.Shared;

namespace MSMES.Domain.WorkOrder;

public class WorkOrder : Entity
{
    public string WorkOrderNo { get; set; } = string.Empty;
    public string? SalesOrderNo { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? ProcessCode { get; set; }
    public string? Note { get; set; }
    public decimal PlannedQuantity { get; set; }
    public decimal ProducedQuantity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Planned;

    public decimal ProgressRate =>
        PlannedQuantity == 0 ? 0 : Math.Min(100, Math.Round(ProducedQuantity / PlannedQuantity * 100, 1));

    public string StatusName => Status switch
    {
        WorkOrderStatus.Planned    => "계획",
        WorkOrderStatus.Released   => "해제",
        WorkOrderStatus.InProgress => "진행중",
        WorkOrderStatus.Completed  => "완료",
        WorkOrderStatus.Closed     => "마감",
        WorkOrderStatus.Cancelled  => "취소",
        _                          => Status.ToString()
    };

    public string StatusCss => Status switch
    {
        WorkOrderStatus.Planned    => "secondary",
        WorkOrderStatus.Released   => "info",
        WorkOrderStatus.InProgress => "primary",
        WorkOrderStatus.Completed  => "success",
        WorkOrderStatus.Closed     => "dark",
        WorkOrderStatus.Cancelled  => "danger",
        _                          => "secondary"
    };

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

    public void Cancel()
    {
        if (Status == WorkOrderStatus.Completed || Status == WorkOrderStatus.Closed)
            throw new InvalidOperationException("Completed or Closed WOs cannot be cancelled.");
        Status = WorkOrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
