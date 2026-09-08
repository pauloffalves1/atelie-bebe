using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Repositories;
using AtelieBebe.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AtelieBebe.Infrastructure.Persistence.Repositories;

public sealed class NewsletterSubscriberRepository : INewsletterSubscriberRepository
{
    private readonly AppDbContext _dbContext;

    public NewsletterSubscriberRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<NewsletterSubscriber?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = Email.Create(email);
        return _dbContext.NewsletterSubscribers.FirstOrDefaultAsync(s => s.Email == normalized, ct);
    }

    public async Task<IReadOnlyList<NewsletterSubscriber>> ListAsync(CancellationToken ct = default) =>
        await _dbContext.NewsletterSubscribers.OrderByDescending(s => s.CreatedAt).ToListAsync(ct);

    public void Add(NewsletterSubscriber subscriber) => _dbContext.NewsletterSubscribers.Add(subscriber);
}
