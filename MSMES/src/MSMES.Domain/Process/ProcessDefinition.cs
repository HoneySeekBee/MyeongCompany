using MSMES.Domain.Shared;

namespace MSMES.Domain.Process;

public class ProcessDefinition : Entity
{
    public string ProcessCode { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public int ProcessOrder { get; set; }
    public decimal StandardTimeMinutes { get; set; }
    public string EquipmentType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
