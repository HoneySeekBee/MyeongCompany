using Dapper;

namespace MSMES.Infrastructure.Persistence;

public static class NumberSequence
{
    public static async Task<string> NextAsync(ISqlConnectionFactory factory, string key, string prefix, CancellationToken ct)
    {
        using var conn = factory.Create();
        const string sql = @"
UPDATE dbo.NumberSequences SET CurrentValue = CurrentValue + 1
OUTPUT inserted.CurrentValue
WHERE SequenceKey = @key;";
        var next = await conn.ExecuteScalarAsync<long>(new CommandDefinition(sql, new { key }, cancellationToken: ct));
        return $"{prefix}{DateTime.UtcNow:yyyyMMdd}{next:D5}";
    }
}
