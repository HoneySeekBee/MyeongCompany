namespace MSMES.Domain.Settings;

public interface ISettingsRepository
{
    /// <summary>모든 설정 키-값 쌍을 반환합니다.</summary>
    Task<Dictionary<string, string>> GetAllAsync(CancellationToken ct = default);

    /// <summary>단일 설정 값을 추가하거나 갱신합니다.</summary>
    Task UpsertAsync(string key, string value, string updatedBy, CancellationToken ct = default);

    /// <summary>여러 설정 값을 일괄 추가/갱신합니다.</summary>
    Task UpsertManyAsync(Dictionary<string, string> settings, string updatedBy, CancellationToken ct = default);
}
