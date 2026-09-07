namespace AtelieBebe.Application.Reviews;

public interface IReviewService
{
    Task<IReadOnlyList<ProductReviewDto>> ListByProductAsync(Guid productId, CancellationToken ct = default);
    Task<ReviewEligibilityDto> GetEligibilityAsync(Guid productId, Guid customerId, CancellationToken ct = default);
    Task<ProductReviewDto> CreateAsync(Guid productId, Guid customerId, CreateReviewRequest request, CancellationToken ct = default);
}
