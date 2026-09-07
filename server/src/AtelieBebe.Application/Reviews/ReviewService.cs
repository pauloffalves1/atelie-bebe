using AtelieBebe.Application.Abstractions;
using AtelieBebe.Application.Exceptions;
using AtelieBebe.Domain.Entities;

namespace AtelieBebe.Application.Reviews;

public sealed class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReviewService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<ProductReviewDto>> ListByProductAsync(Guid productId, CancellationToken ct = default)
    {
        var reviews = await _unitOfWork.ProductReviews.ListByProductAsync(productId, ct);
        return reviews.Select(ToDto).ToList();
    }

    public async Task<ReviewEligibilityDto> GetEligibilityAsync(Guid productId, Guid customerId, CancellationToken ct = default)
    {
        var hasPurchased = await _unitOfWork.Orders.CustomerHasPurchasedProductAsync(customerId, productId, ct);
        var alreadyReviewed = await _unitOfWork.ProductReviews.ExistsAsync(productId, customerId, ct);
        return new ReviewEligibilityDto(hasPurchased, alreadyReviewed);
    }

    public async Task<ProductReviewDto> CreateAsync(Guid productId, Guid customerId, CreateReviewRequest request, CancellationToken ct = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId, ct)
            ?? throw new NotFoundException("Produto", productId);

        var hasPurchased = await _unitOfWork.Orders.CustomerHasPurchasedProductAsync(customerId, productId, ct);
        if (!hasPurchased)
            throw new ConflictException("Você precisa ter comprado este produto para avaliá-lo.");

        var alreadyReviewed = await _unitOfWork.ProductReviews.ExistsAsync(productId, customerId, ct);
        if (alreadyReviewed)
            throw new ConflictException("Você já avaliou este produto.");

        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId, ct)
            ?? throw new NotFoundException("Cliente", customerId);

        var review = ProductReview.Create(product.Id, customerId, customer.Name, request.Rating, request.Comment);
        _unitOfWork.ProductReviews.Add(review);
        await _unitOfWork.SaveChangesAsync(ct);

        return ToDto(review);
    }

    private static ProductReviewDto ToDto(ProductReview r) =>
        new(r.Id, r.ProductId, r.CustomerName, r.Rating, r.Comment, r.CreatedAt);
}
