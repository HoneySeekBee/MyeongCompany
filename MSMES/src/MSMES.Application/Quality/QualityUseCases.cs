using MSMES.Domain.Quality;

namespace MSMES.Application.Quality;

public sealed record CreateQualityInspectionCommand(
    string LotNo,
    string InspectionItem,
    decimal InspectedQuantity,
    decimal DefectQuantity,
    string? DefectTypeCode,
    QualityStatus Result,
    string Inspector,
    string? Remarks,
    string CreatedBy);

public sealed class CreateQualityInspectionHandler
{
    private readonly IQualityRepository _repo;
    public CreateQualityInspectionHandler(IQualityRepository repo) => _repo = repo;

    public async Task<string> HandleAsync(CreateQualityInspectionCommand cmd, CancellationToken ct = default)
    {
        if (cmd.DefectQuantity > cmd.InspectedQuantity)
            throw new InvalidOperationException("Defect quantity exceeds inspected quantity");

        var no = await _repo.NextNumberAsync(ct);
        var insp = new QualityInspection
        {
            InspectionNo = no,
            LotNo = cmd.LotNo,
            InspectionItem = cmd.InspectionItem,
            InspectedQuantity = cmd.InspectedQuantity,
            DefectQuantity = cmd.DefectQuantity,
            DefectTypeCode = cmd.DefectTypeCode,
            Result = cmd.Result,
            Inspector = cmd.Inspector,
            Remarks = cmd.Remarks,
            InspectionDate = DateTime.UtcNow,
            CreatedBy = cmd.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };
        await _repo.AddAsync(insp, ct);
        return no;
    }
}

public sealed record QualityReportDto(
    DateTime From,
    DateTime To,
    decimal TotalInspected,
    decimal TotalDefect,
    decimal DefectRate,
    IReadOnlyList<QualityInspection> Details);

public sealed record GetQualityReportQuery(DateTime From, DateTime To);

public sealed class GetQualityReportHandler
{
    private readonly IQualityRepository _repo;
    public GetQualityReportHandler(IQualityRepository repo) => _repo = repo;

    public async Task<QualityReportDto> HandleAsync(GetQualityReportQuery q, CancellationToken ct = default)
    {
        var stats = await _repo.GetDefectStatsAsync(q.From, q.To, ct);
        var details = await _repo.ListByDateAsync(q.From, q.To, ct);
        return new QualityReportDto(q.From, q.To, stats.TotalInspected, stats.TotalDefect, stats.DefectRate, details);
    }
}
