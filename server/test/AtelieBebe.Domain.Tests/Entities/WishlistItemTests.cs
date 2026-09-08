using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Exceptions;

namespace AtelieBebe.Domain.Tests.Entities;

public class WishlistItemTests
{
    [Fact]
    public void Create_WithEmptyCustomerId_Throws()
    {
        Assert.Throws<DomainException>(() => WishlistItem.Create(Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public void Create_WithEmptyProductId_Throws()
    {
        Assert.Throws<DomainException>(() => WishlistItem.Create(Guid.NewGuid(), Guid.Empty));
    }

    [Fact]
    public void Create_Valid_SetsFields()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var item = WishlistItem.Create(customerId, productId);

        Assert.Equal(customerId, item.CustomerId);
        Assert.Equal(productId, item.ProductId);
    }
}
