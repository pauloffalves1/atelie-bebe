using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AtelieBebe.Infrastructure.Persistence.Repositories;

public sealed class CartSnapshotRepository : ICartSnapshotRepository
{
    private readonly AppDbContext _dbContext;

    public CartSnapshotRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<CartSnapshot?> GetByCustomerAsync(Guid customerId, CancellationToken ct = default) =>
        _dbContext.CartSnapshots.FirstOrDefaultAsync(c => c.CustomerId == customerId, ct);

    public async Task<IReadOnlyList<CartSnapshot>> ListAbandonedAsync(DateTime updatedBeforeUtc, CancellationToken ct = default) =>
        await _dbContext.CartSnapshots
            .Where(c => c.UpdatedAt < updatedBeforeUtc && c.ReminderSentAt == null && c.ItemsJson != "[]")
            .ToListAsync(ct);

    public void Add(CartSnapshot snapshot) => _dbContext.CartSnapshots.Add(snapshot);

    public void Remove(CartSnapshot snapshot) => _dbContext.CartSnapshots.Remove(snapshot);
}
