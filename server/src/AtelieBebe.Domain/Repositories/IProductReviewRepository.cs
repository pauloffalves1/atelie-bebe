using AtelieBebe.Domain.Entities;

namespace AtelieBebe.Domain.Repositories;

public interface IProductReviewRepository
{
    Task<IReadOnlyList<ProductReview>> ListByProductAsync(Guid productId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid productId, Guid customerId, CancellationToken ct = default);
    void Add(ProductReview review);
}
