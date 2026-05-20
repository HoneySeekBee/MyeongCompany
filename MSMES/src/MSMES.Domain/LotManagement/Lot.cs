using MSMES.Domain.Shared;

namespace MSMES.Domain.LotManagement;

public class Lot : Entity
{
    public string LotNo { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string WorkOrderNo { get; set; } = string.Empty;
    public decimal ProducedQuantity { get; set; }
    public decimal DefectQuantity { get; set; }
    public DateTime ProductionDate { get; set; }
    public LotStatus Status { get; set; } = LotStatus.Created;

    public List<LotHistory> Histories { get; set; } = new();

    public void AddHistory(string operation, string @operator, string? remarks = null)
    {
        Histories.Add(new LotHistory
        {
            LotNo = LotNo,
            Operation = operation,
            OperatedAt = DateTime.UtcNow,
            Operator = @operator,
            Remarks = remarks
        });
        UpdatedAt = DateTime.UtcNow;
    }

    public void Release()
    {
        Status = LotStatus.Released;
        UpdatedAt = DateTime.UtcNow;
    }
}
