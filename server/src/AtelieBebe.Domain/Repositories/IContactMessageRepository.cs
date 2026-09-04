using AtelieBebe.Domain.Entities;

namespace AtelieBebe.Domain.Repositories;

public interface IContactMessageRepository
{
    void Add(ContactMessage message);
    Task<IReadOnlyList<ContactMessage>> ListAsync(CancellationToken ct = default);
}
