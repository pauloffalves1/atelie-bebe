using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Exceptions;

namespace AtelieBebe.Domain.Tests.Entities;

public class ProductReviewTests
{
    private static readonly Guid ProductId = Guid.NewGuid();
    private static readonly Guid CustomerId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        var review = ProductReview.Create(ProductId, CustomerId, "Maria Silva", 5, "Ficou linda, adorei o bordado!");

        Assert.Equal(ProductId, review.ProductId);
        Assert.Equal(CustomerId, review.CustomerId);
        Assert.Equal("Maria Silva", review.CustomerName);
        Assert.Equal(5, review.Rating);
        Assert.Equal("Ficou linda, adorei o bordado!", review.Comment);
    }

    [Fact]
    public void Create_WithoutComment_SucceedsWithNullComment()
    {
        var review = ProductReview.Create(ProductId, CustomerId, "Maria Silva", 4, null);

        Assert.Null(review.Comment);
    }

    [Fact]
    public void Create_WithBlankComment_NormalizesToNull()
    {
        var review = ProductReview.Create(ProductId, CustomerId, "Maria Silva", 4, "   ");

        Assert.Null(review.Comment);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Create_WithRatingOutOfRange_Throws(int rating)
    {
        Assert.Throws<DomainException>(() => ProductReview.Create(ProductId, CustomerId, "Maria Silva", rating, null));
    }

    [Fact]
    public void Create_WithEmptyProductId_Throws()
    {
        Assert.Throws<DomainException>(() => ProductReview.Create(Guid.Empty, CustomerId, "Maria Silva", 5, null));
    }

    [Fact]
    public void Create_WithEmptyCustomerId_Throws()
    {
        Assert.Throws<DomainException>(() => ProductReview.Create(ProductId, Guid.Empty, "Maria Silva", 5, null));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyCustomerName_Throws(string? name)
    {
        Assert.Throws<DomainException>(() => ProductReview.Create(ProductId, CustomerId, name!, 5, null));
    }
}
