using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AtelieBebe.Infrastructure.Persistence.Repositories;

public sealed class ProductReviewRepository : IProductReviewRepository
{
    private readonly AppDbContext _dbContext;

    public ProductReviewRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<ProductReview>> ListByProductAsync(Guid productId, CancellationToken ct = default) =>
        await _dbContext.ProductReviews
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public Task<bool> ExistsAsync(Guid productId, Guid customerId, CancellationToken ct = default) =>
        _dbContext.ProductReviews.AnyAsync(r => r.ProductId == productId && r.CustomerId == customerId, ct);

    public void Add(ProductReview review) => _dbContext.ProductReviews.Add(review);
}
