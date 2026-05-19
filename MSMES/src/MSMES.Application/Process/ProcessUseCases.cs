using MSMES.Domain.Process;

namespace MSMES.Application.Process;

public sealed record CreateProductionResultCommand(
    string WorkOrderNo,
    string ProcessCode,
    string Operator,
    decimal ProducedQuantity,
    decimal DefectQuantity,
    DateTime StartTime,
    DateTime? EndTime,
    string CreatedBy);

public sealed class CreateProductionResultHandler
{
    private readonly IProcessRepository _repo;
    public CreateProductionResultHandler(IProcessRepository repo) => _repo = repo;

    public async Task<string> HandleAsync(CreateProductionResultCommand cmd, CancellationToken ct = default)
    {
        var no = await _repo.NextResultNoAsync(ct);
        var result = new ProductionResult
        {
            ResultNo = no,
            WorkOrderNo = cmd.WorkOrderNo,
            ProcessCode = cmd.ProcessCode,
            Operator = cmd.Operator,
            ProducedQuantity = cmd.ProducedQuantity,
            DefectQuantity = cmd.DefectQuantity,
            StartTime = cmd.StartTime,
            EndTime = cmd.EndTime,
            CreatedBy = cmd.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };
        await _repo.AddResultAsync(result, ct);
        return no;
    }
}

public sealed record ProductionSummaryDto(
    DateTime From,
    DateTime To,
    decimal TotalProduced,
    decimal TotalDefect,
    decimal DefectRate,
    IReadOnlyList<ProductionResult> Results);

public sealed record GetProductionResultQuery(DateTime From, DateTime To);

public sealed class GetProductionResultHandler
{
    private readonly IProcessRepository _repo;
    public GetProductionResultHandler(IProcessRepository repo) => _repo = repo;

    public async Task<ProductionSummaryDto> HandleAsync(GetProductionResultQuery q, CancellationToken ct = default)
    {
        var results = await _repo.ListResultsAsync(q.From, q.To, ct);
        var totalProduced = results.Sum(r => r.ProducedQuantity);
        var totalDefect = results.Sum(r => r.DefectQuantity);
        var rate = totalProduced == 0 ? 0 : Math.Round(totalDefect / totalProduced * 100m, 2);
        return new ProductionSummaryDto(q.From, q.To, totalProduced, totalDefect, rate, results);
    }
}
