namespace MSMES.Domain.Common;

public interface IUserRepository
{
    Task<User?> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
}

public interface ICommonCodeRepository
{
    Task<IReadOnlyList<CommonCode>> ListByGroupAsync(string codeGroup, CancellationToken ct = default);
    Task AddAsync(CommonCode code, CancellationToken ct = default);
}
