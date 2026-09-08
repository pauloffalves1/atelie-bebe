using AtelieBebe.Domain.Entities;

namespace AtelieBebe.Domain.Repositories;

public interface INewsletterSubscriberRepository
{
    Task<NewsletterSubscriber?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<NewsletterSubscriber>> ListAsync(CancellationToken ct = default);
    void Add(NewsletterSubscriber subscriber);
}
