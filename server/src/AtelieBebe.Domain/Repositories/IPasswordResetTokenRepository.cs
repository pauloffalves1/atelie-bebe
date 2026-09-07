using AtelieBebe.Domain.Entities;

namespace AtelieBebe.Domain.Repositories;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    void Add(PasswordResetToken token);
}
