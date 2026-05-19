using Dapper;
using MSMES.Domain.Common;
using MSMES.Infrastructure.Persistence;

namespace MSMES.Infrastructure.Repositories;

public sealed class SqlUserRepository : IUserRepository
{
    private readonly ISqlConnectionFactory _factory;
    public SqlUserRepository(ISqlConnectionFactory factory) => _factory = factory;

    public async Task<User?> GetByUserIdAsync(string userId, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        return await conn.QuerySingleOrDefaultAsync<User>(new CommandDefinition(
            "SELECT * FROM dbo.Users WHERE UserId=@id", new { id = userId }, cancellationToken: ct));
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        return await conn.QuerySingleOrDefaultAsync<User>(new CommandDefinition(
            "SELECT * FROM dbo.Users WHERE Email=@email", new { email }, cancellationToken: ct));
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"INSERT INTO dbo.Users
            (UserId, Name, Email, Role, PasswordHash, IsActive, CreatedAt, CreatedBy)
            VALUES (@UserId,@Name,@Email,@Role,@PasswordHash,@IsActive,@CreatedAt,@CreatedBy)";
        await conn.ExecuteAsync(new CommandDefinition(sql, user, cancellationToken: ct));
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"UPDATE dbo.Users SET Name=@Name, Email=@Email, Role=@Role,
            PasswordHash=@PasswordHash, IsActive=@IsActive, UpdatedAt=SYSUTCDATETIME() WHERE UserId=@UserId";
        await conn.ExecuteAsync(new CommandDefinition(sql, user, cancellationToken: ct));
    }
}

public sealed class SqlCommonCodeRepository : ICommonCodeRepository
{
    private readonly ISqlConnectionFactory _factory;
    public SqlCommonCodeRepository(ISqlConnectionFactory factory) => _factory = factory;

    public async Task<IReadOnlyList<CommonCode>> ListByGroupAsync(string codeGroup, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        var rows = await conn.QueryAsync<CommonCode>(new CommandDefinition(
            "SELECT * FROM dbo.CommonCodes WHERE CodeGroup=@g AND IsActive=1 ORDER BY SortOrder, Code",
            new { g = codeGroup }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task AddAsync(CommonCode code, CancellationToken ct = default)
    {
        using var conn = _factory.Create();
        const string sql = @"INSERT INTO dbo.CommonCodes
            (CodeGroup, Code, CodeName, SortOrder, IsActive, CreatedAt, CreatedBy)
            VALUES (@CodeGroup,@Code,@CodeName,@SortOrder,@IsActive,@CreatedAt,@CreatedBy)";
        await conn.ExecuteAsync(new CommandDefinition(sql, code, cancellationToken: ct));
    }
}
