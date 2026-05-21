namespace MSMES.Domain.Equipment;

public class OeeRecord
{
    public int     Id                  { get; set; }
    public int     EquipmentId         { get; set; }
    public string  EquipmentName       { get; set; } = string.Empty;
    public DateTime RecordDate         { get; set; }
    public int     PlannedTimeMinutes  { get; set; }
    public int     ActualTimeMinutes   { get; set; }
    public decimal IdealCyclePerMinute { get; set; }
    public decimal ActualProduced      { get; set; }
    public decimal GoodQuantity        { get; set; }

    // Calculated
    public decimal Availability => PlannedTimeMinutes == 0 ? 0 :
        Math.Round((decimal)ActualTimeMinutes / PlannedTimeMinutes * 100m, 1);
    public decimal Performance => ActualTimeMinutes == 0 ? 0 :
        Math.Round(ActualProduced / (IdealCyclePerMinute * ActualTimeMinutes) * 100m, 1);
    public decimal QualityRate => ActualProduced == 0 ? 0 :
        Math.Round(GoodQuantity / ActualProduced * 100m, 1);
    public decimal OEE => Math.Round(Availability * Performance * QualityRate / 10000m, 1);
}
