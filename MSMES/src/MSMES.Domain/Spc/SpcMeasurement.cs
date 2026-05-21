namespace MSMES.Domain.Spc;

public class SpcMeasurement
{
    public int     Id            { get; set; }
    public string  ProcessCode   { get; set; } = string.Empty;
    public string  ProcessName   { get; set; } = string.Empty;
    public string  CharName      { get; set; } = string.Empty;
    public decimal NominalValue  { get; set; }
    public decimal UpperSpec     { get; set; }
    public decimal LowerSpec     { get; set; }
    public decimal MeasuredValue { get; set; }
    public int     SubgroupNo    { get; set; }
    public int     SampleNo      { get; set; }
    public string? MeasuredBy    { get; set; }
    public DateTime MeasuredAt   { get; set; }
    public string? LotNo         { get; set; }
}

public interface ISpcRepository
{
    Task<IReadOnlyList<SpcMeasurement>> GetByProcessAsync(string processCode, int subgroupCount, CancellationToken ct = default);
    Task<IReadOnlyList<string>>         GetProcessListAsync(CancellationToken ct = default);
}
