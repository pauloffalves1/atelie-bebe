using AtelieBebe.Domain.Entities;

namespace AtelieBebe.Domain.Repositories;

public interface IEmailVerificationTokenRepository
{
    Task<EmailVerificationToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    void Add(EmailVerificationToken token);
}
