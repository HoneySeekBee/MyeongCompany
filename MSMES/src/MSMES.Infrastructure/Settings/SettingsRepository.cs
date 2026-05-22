using Dapper;
using MSMES.Domain.Settings;
using MSMES.Infrastructure.Persistence;

namespace MSMES.Infrastructure.Settings;

public sealed class SettingsRepository : ISettingsRepository
{
    private readonly ISqlConnectionFactory _factory;

    public SettingsRepository(ISqlConnectionFactory factory) => _factory = factory;

    public async Task<Dictionary<string, string>> GetAllAsync(CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        var rows = await conn.QueryAsync<(string Key, string? Value)>(new CommandDefinition(
            "SELECT SettingKey, SettingValue FROM dbo.SystemSettings ORDER BY SettingKey",
            cancellationToken: ct));

        return rows.ToDictionary(
            r => r.Key,
            r => r.Value ?? string.Empty);
    }

    public async Task UpsertAsync(string key, string value, string updatedBy, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"
            MERGE dbo.SystemSettings WITH (HOLDLOCK) AS target
            USING (SELECT @key AS SettingKey) AS source
                ON target.SettingKey = source.SettingKey
            WHEN MATCHED THEN
                UPDATE SET
                    SettingValue = @value,
                    UpdatedAt    = SYSUTCDATETIME(),
                    UpdatedBy    = @updatedBy
            WHEN NOT MATCHED THEN
                INSERT (SettingKey, SettingValue, UpdatedAt, UpdatedBy)
                VALUES (@key, @value, SYSUTCDATETIME(), @updatedBy);";

        await conn.ExecuteAsync(new CommandDefinition(sql,
            new { key, value, updatedBy },
            cancellationToken: ct));
    }

    public async Task UpsertManyAsync(
        Dictionary<string, string> settings,
        string updatedBy,
        CancellationToken ct = default)
    {
        if (settings.Count == 0) return;

        using var conn = _factory.Create();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            const string sql = @"
                MERGE dbo.SystemSettings WITH (HOLDLOCK) AS target
                USING (SELECT @key AS SettingKey) AS source
                    ON target.SettingKey = source.SettingKey
                WHEN MATCHED THEN
                    UPDATE SET
                        SettingValue = @value,
                        UpdatedAt    = SYSUTCDATETIME(),
                        UpdatedBy    = @updatedBy
                WHEN NOT MATCHED THEN
                    INSERT (SettingKey, SettingValue, UpdatedAt, UpdatedBy)
                    VALUES (@key, @value, SYSUTCDATETIME(), @updatedBy);";

            foreach (var (key, value) in settings)
            {
                await conn.ExecuteAsync(new CommandDefinition(sql,
                    new { key, value, updatedBy },
                    transaction: tx,
                    cancellationToken: ct));
            }

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}
