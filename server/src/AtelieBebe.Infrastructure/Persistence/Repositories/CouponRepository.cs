using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AtelieBebe.Infrastructure.Persistence.Repositories;

public sealed class CouponRepository : ICouponRepository
{
    private readonly AppDbContext _dbContext;

    public CouponRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return _dbContext.Coupons.FirstOrDefaultAsync(c => c.Code == normalized, ct);
    }

    public Task<Coupon?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _dbContext.Coupons.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Coupon>> ListAsync(CancellationToken ct = default) =>
        await _dbContext.Coupons.OrderByDescending(c => c.CreatedAt).ToListAsync(ct);

    public void Add(Coupon coupon) => _dbContext.Coupons.Add(coupon);
}
