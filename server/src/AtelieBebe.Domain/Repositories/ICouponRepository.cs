using AtelieBebe.Domain.Entities;

namespace AtelieBebe.Domain.Repositories;

public interface ICouponRepository
{
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Coupon?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Coupon>> ListAsync(CancellationToken ct = default);
    void Add(Coupon coupon);
}
