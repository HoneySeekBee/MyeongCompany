namespace MSMES.Domain.Quality;

public interface IQualityRepository
{
    Task<QualityInspection?> GetByNoAsync(string inspectionNo, CancellationToken ct = default);
    Task<IReadOnlyList<QualityInspection>> ListAsync(int skip, int take, CancellationToken ct = default);
    Task<IReadOnlyList<QualityInspection>> ListByDateAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task AddAsync(QualityInspection inspection, CancellationToken ct = default);
    Task UpdateAsync(QualityInspection inspection, CancellationToken ct = default);
    Task<string> NextNumberAsync(CancellationToken ct = default);

    Task<IReadOnlyList<DefectType>> ListDefectTypesAsync(CancellationToken ct = default);
    Task AddDefectTypeAsync(DefectType type, CancellationToken ct = default);

    Task<(decimal TotalInspected, decimal TotalDefect, decimal DefectRate)> GetDefectStatsAsync(DateTime from, DateTime to, CancellationToken ct = default);
}
