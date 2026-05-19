namespace MSMES.Domain.Process;

public interface IProcessRepository
{
    Task<ProcessDefinition?> GetProcessAsync(string processCode, CancellationToken ct = default);
    Task<IReadOnlyList<ProcessDefinition>> ListProcessesAsync(CancellationToken ct = default);
    Task AddProcessAsync(ProcessDefinition process, CancellationToken ct = default);
    Task UpdateProcessAsync(ProcessDefinition process, CancellationToken ct = default);

    Task<string> NextResultNoAsync(CancellationToken ct = default);
    Task AddResultAsync(ProductionResult result, CancellationToken ct = default);
    Task<IReadOnlyList<ProductionResult>> ListResultsAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<IReadOnlyList<ProductionResult>> ListResultsByWorkOrderAsync(string workOrderNo, CancellationToken ct = default);

    Task<(decimal Produced, decimal Defect)> GetDailyTotalsAsync(DateTime date, CancellationToken ct = default);
}
