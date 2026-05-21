namespace MSMES.Domain.LotManagement;

public interface ILotRepository
{
    Task<Lot?> GetByNoAsync(string lotNo, CancellationToken ct = default);
    Task<IReadOnlyList<Lot>> ListAsync(int skip, int take, CancellationToken ct = default);
    Task<IReadOnlyList<Lot>> ListAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<LotHistory>> GetHistoryAsync(string lotNo, CancellationToken ct = default);
    Task AddAsync(Lot lot, CancellationToken ct = default);
    Task UpdateAsync(Lot lot, CancellationToken ct = default);
    Task AddHistoryAsync(LotHistory history, CancellationToken ct = default);
    Task<string> NextNumberAsync(CancellationToken ct = default);
    Task<IReadOnlyList<LotMaterial>>  GetLotMaterialsAsync(string lotNo, CancellationToken ct = default);
    Task<IReadOnlyList<string>>       FindLotsByMaterialAsync(string materialCode, CancellationToken ct = default);
}
