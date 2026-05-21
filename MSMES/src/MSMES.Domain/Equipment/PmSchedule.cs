namespace MSMES.Domain.Equipment;

public class PmSchedule
{
    public int      Id            { get; set; }
    public int      EquipmentId   { get; set; }
    public string   EquipmentName { get; set; } = string.Empty;
    public string   PmType        { get; set; } = string.Empty;
    public int      IntervalDays  { get; set; }
    public DateTime? LastPmDate   { get; set; }
    public DateTime NextPmDate    { get; set; }
    public string?  AssignedTo    { get; set; }
    public string?  CheckItems    { get; set; }
    public bool     IsActive      { get; set; } = true;
    public DateTime CreatedAt     { get; set; }

    public int DaysUntilDue => (NextPmDate.Date - DateTime.Today).Days;
    public bool IsOverdue   => DaysUntilDue < 0;
    public bool IsDueSoon   => DaysUntilDue >= 0 && DaysUntilDue <= 3;
    public string UrgencyCss => IsOverdue ? "danger" : IsDueSoon ? "warning" : "success";
    public string[] CheckItemList => CheckItems?.Split('\n', StringSplitOptions.RemoveEmptyEntries) ?? [];
}

public class PmRecord
{
    public int      Id             { get; set; }
    public int      PmScheduleId   { get; set; }
    public int      EquipmentId    { get; set; }
    public string   EquipmentName  { get; set; } = string.Empty;
    public DateTime PmDate         { get; set; }
    public string   Technician     { get; set; } = string.Empty;
    public byte     Result         { get; set; }
    public string?  FindingsNote   { get; set; }
    public DateTime? NextPmDate    { get; set; }
    public int      WorkTimeMinutes{ get; set; }
    public DateTime CreatedAt      { get; set; }

    public string ResultName => Result switch { 0 => "정상", 1 => "이상발견", 2 => "수리완료", _ => "-" };
    public string ResultCss  => Result switch { 0 => "success", 1 => "danger", 2 => "warning", _ => "secondary" };
}
