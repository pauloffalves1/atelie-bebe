namespace AtelieBebe.Application.Reviews;

public sealed record ProductReviewDto(
    Guid Id,
    Guid ProductId,
    string CustomerName,
    int Rating,
    string? Comment,
    DateTime CreatedAt);

public sealed record CreateReviewRequest(int Rating, string? Comment);

/// <summary>Tells the frontend whether to show "write a review" for the current customer/product pair.</summary>
public sealed record ReviewEligibilityDto(bool HasPurchased, bool AlreadyReviewed);
