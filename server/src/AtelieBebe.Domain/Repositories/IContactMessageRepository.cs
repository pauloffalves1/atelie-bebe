using AtelieBebe.Domain.Entities;

namespace AtelieBebe.Domain.Repositories;

public interface IContactMessageRepository
{
    void Add(ContactMessage message);
    Task<(IReadOnlyList<ContactMessage> Items, int TotalItems)> ListAsync(int page, int pageSize, CancellationToken ct = default);
}
