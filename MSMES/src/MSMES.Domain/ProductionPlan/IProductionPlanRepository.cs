namespace MSMES.Domain.ProductionPlan;

public interface IProductionPlanRepository
{
    Task<IReadOnlyList<ProductionPlan>> ListAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<int> CreateAsync(ProductionPlan plan, CancellationToken ct = default);
    Task UpdateStatusAsync(int id, byte status, CancellationToken ct = default);
}
