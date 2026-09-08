using System.Linq;
using AtelieBebe.Application.Abstractions;
using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.ValueObjects;

namespace AtelieBebe.Application.Newsletter;

public sealed class NewsletterService : INewsletterService
{
    private readonly IUnitOfWork _unitOfWork;

    public NewsletterService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task SubscribeAsync(string email, CancellationToken ct = default)
    {
        var normalized = Email.Create(email);
        var existing = await _unitOfWork.NewsletterSubscribers.GetByEmailAsync(normalized.Value, ct);

        if (existing is null)
            _unitOfWork.NewsletterSubscribers.Add(NewsletterSubscriber.Create(normalized));
        else if (!existing.Active)
            existing.Reactivate();
        else
            return; // already an active subscriber, nothing to do

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<NewsletterSubscriberDto>> ListAsync(CancellationToken ct = default)
    {
        var subscribers = await _unitOfWork.NewsletterSubscribers.ListAsync(ct);
        return subscribers.Where(s => s.Active).Select(s => new NewsletterSubscriberDto(s.Id, s.Email.Value, s.CreatedAt)).ToList();
    }
}
