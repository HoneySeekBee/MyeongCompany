namespace MSMES.Domain.ProductionPlan;

public class ProductionPlan
{
    public int     Id          { get; set; }
    public string  PlanNo      { get; set; } = string.Empty;
    public string? WorkOrderNo { get; set; }
    public string  ProductCode { get; set; } = string.Empty;
    public string  ProductName { get; set; } = string.Empty;
    public decimal PlannedQty  { get; set; }
    public DateTime StartDate  { get; set; }
    public DateTime EndDate    { get; set; }
    public string? Line        { get; set; }
    public byte    Status      { get; set; }
    public byte    Priority    { get; set; } = 1;
    public string? Remark      { get; set; }
    public DateTime CreatedAt  { get; set; }
    public string? CreatedBy   { get; set; }

    public int DurationDays => Math.Max(1, (EndDate - StartDate).Days + 1);
    public string StatusName => Status switch { 0 => "계획", 1 => "진행중", 2 => "완료", 3 => "취소", _ => "-" };
    public string PriorityName => Priority switch { 1 => "보통", 2 => "높음", 3 => "긴급", _ => "-" };
}
