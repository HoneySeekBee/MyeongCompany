using MSMES.Domain.Shared;

namespace MSMES.Domain.Equipment;

public class EquipmentMaintenance : Entity
{
    public string MaintenanceNo { get; set; } = string.Empty;
    public string EquipmentCode { get; set; } = string.Empty;
    public string MaintenanceType { get; set; } = string.Empty; // 정기/긴급/예방
    public string Description { get; set; } = string.Empty;
    public DateTime MaintenanceDate { get; set; } = DateTime.UtcNow;
    public string Operator { get; set; } = string.Empty;
    public DateTime? NextDueDate { get; set; }
}
