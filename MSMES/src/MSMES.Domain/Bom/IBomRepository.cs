namespace MSMES.Domain.Bom;

public interface IBomRepository
{
    Task<IReadOnlyList<Bom>> ListAsync(CancellationToken ct = default);
    Task<Bom?>               GetAsync(int id, CancellationToken ct = default);
    Task<int>                CreateAsync(Bom bom, CancellationToken ct = default);
    Task                     UpdateAsync(Bom bom, CancellationToken ct = default);
    Task                     DeleteAsync(int id, CancellationToken ct = default);
    Task<string>             NextBomNoAsync(CancellationToken ct = default);
}
