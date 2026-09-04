using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AtelieBebe.Infrastructure.Persistence.Repositories;

public sealed class ContactMessageRepository : IContactMessageRepository
{
    private readonly AppDbContext _dbContext;

    public ContactMessageRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public void Add(ContactMessage message) => _dbContext.ContactMessages.Add(message);

    public async Task<IReadOnlyList<ContactMessage>> ListAsync(CancellationToken ct = default) =>
        await _dbContext.ContactMessages.ToListAsync(ct);
}
