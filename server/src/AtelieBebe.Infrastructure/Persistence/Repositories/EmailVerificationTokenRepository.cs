using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AtelieBebe.Infrastructure.Persistence.Repositories;

public sealed class EmailVerificationTokenRepository : IEmailVerificationTokenRepository
{
    private readonly AppDbContext _dbContext;

    public EmailVerificationTokenRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<EmailVerificationToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default) =>
        _dbContext.EmailVerificationTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public void Add(EmailVerificationToken token) => _dbContext.EmailVerificationTokens.Add(token);
}
