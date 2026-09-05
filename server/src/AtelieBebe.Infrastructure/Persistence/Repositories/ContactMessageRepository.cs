using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AtelieBebe.Infrastructure.Persistence.Repositories;

public sealed class ContactMessageRepository : IContactMessageRepository
{
    private readonly AppDbContext _dbContext;

    public ContactMessageRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public void Add(ContactMessage message) => _dbContext.ContactMessages.Add(message);

    public async Task<(IReadOnlyList<ContactMessage> Items, int TotalItems)> ListAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbContext.ContactMessages.OrderByDescending(m => m.CreatedAt);

        var totalItems = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (items, totalItems);
    }
}
