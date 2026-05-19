using MSMES.Domain.Shared;

namespace MSMES.Domain.Equipment;

public class Equipment : Entity
{
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public string EquipmentType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public EquipmentStatus Status { get; set; } = EquipmentStatus.Stopped;
    public DateTime? LastInspectionDate { get; set; }
    public DateTime? NextInspectionDate { get; set; }

    public void ChangeStatus(EquipmentStatus next)
    {
        Status = next;
        UpdatedAt = DateTime.UtcNow;
    }
}
