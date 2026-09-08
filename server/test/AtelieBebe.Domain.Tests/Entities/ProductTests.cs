using System.Linq;
using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Events;
using AtelieBebe.Domain.Exceptions;
using AtelieBebe.Domain.ValueObjects;

namespace AtelieBebe.Domain.Tests.Entities;

public class ProductTests
{
    private static Product CreateProduct() =>
        Product.Create("Body Manga Longa", "body-manga-longa", "Descrição", Money.FromReais(69.90m),
            "Bodies", imageUrl: null);

    [Theory]
    [InlineData("", "slug", "categoria")]
    [InlineData("nome", "", "categoria")]
    [InlineData("nome", "slug", "")]
    public void Create_WithMissingRequiredField_Throws(string name, string slug, string category)
    {
        Assert.Throws<DomainException>(() =>
            Product.Create(name, slug, "descrição", Money.FromReais(10m), category, null));
    }

    [Fact]
    public void Create_IsActiveByDefault()
    {
        var product = CreateProduct();

        Assert.True(product.Active);
    }

    [Fact]
    public void SetActive_TogglesFlag()
    {
        var product = CreateProduct();

        product.SetActive(false);

        Assert.False(product.Active);
    }

    [Fact]
    public void SetActive_ReactivatingAfterDeactivation_RaisesBackInStockEvent()
    {
        var product = CreateProduct();
        product.SetActive(false);
        product.ClearDomainEvents();

        product.SetActive(true);

        var raised = Assert.Single(product.DomainEvents.OfType<ProductBackInStockDomainEvent>());
        Assert.Equal(product.Id, raised.ProductId);
        Assert.Equal(product.Slug, raised.ProductSlug);
    }

    [Fact]
    public void SetActive_AlreadyActive_DoesNotRaiseBackInStockEvent()
    {
        var product = CreateProduct();
        product.ClearDomainEvents();

        product.SetActive(true);

        Assert.Empty(product.DomainEvents.OfType<ProductBackInStockDomainEvent>());
    }

    [Fact]
    public void SetActive_Deactivating_DoesNotRaiseBackInStockEvent()
    {
        var product = CreateProduct();
        product.ClearDomainEvents();

        product.SetActive(false);

        Assert.Empty(product.DomainEvents.OfType<ProductBackInStockDomainEvent>());
    }

    [Fact]
    public void NewProduct_IsPublicByDefault()
    {
        var product = CreateProduct();

        Assert.False(product.IsExclusive);
        Assert.Empty(product.AllowedCustomerIds);
        Assert.True(product.HasAccess(null));
        Assert.True(product.HasAccess(Guid.NewGuid()));
    }

    [Fact]
    public void SetAllowedCustomers_WithAtLeastOneId_MakesProductExclusive()
    {
        var product = CreateProduct();
        var allowedCustomer = Guid.NewGuid();

        product.SetAllowedCustomers([allowedCustomer]);

        Assert.True(product.IsExclusive);
        Assert.True(product.HasAccess(allowedCustomer));
        Assert.False(product.HasAccess(Guid.NewGuid()));
        Assert.False(product.HasAccess(null));
    }

    [Fact]
    public void SetAllowedCustomers_DeduplicatesIds()
    {
        var product = CreateProduct();
        var customer = Guid.NewGuid();

        product.SetAllowedCustomers([customer, customer, customer]);

        Assert.Single(product.AllowedCustomerIds);
    }

    [Fact]
    public void SetAllowedCustomers_WithEmptyCollection_MakesProductPublicAgain()
    {
        var product = CreateProduct();
        product.SetAllowedCustomers([Guid.NewGuid()]);

        product.SetAllowedCustomers([]);

        Assert.False(product.IsExclusive);
        Assert.True(product.HasAccess(null));
    }

    [Fact]
    public void SetAllowedCustomers_ReplacesThePreviousSetEntirely()
    {
        var product = CreateProduct();
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        product.SetAllowedCustomers([first]);

        product.SetAllowedCustomers([second]);

        Assert.False(product.HasAccess(first));
        Assert.True(product.HasAccess(second));
    }

    [Fact]
    public void SetImages_Valid_KeepsGivenOrder()
    {
        var product = CreateProduct();

        product.SetImages(["/api/uploads/products/a.jpg", "/api/uploads/products/b.jpg"]);

        Assert.Equal(["/api/uploads/products/a.jpg", "/api/uploads/products/b.jpg"], product.ImageUrls);
    }

    [Fact]
    public void SetImages_ReplacesThePreviousSetEntirely()
    {
        var product = CreateProduct();
        product.SetImages(["/api/uploads/products/a.jpg"]);

        product.SetImages(["/api/uploads/products/b.jpg"]);

        Assert.Equal(["/api/uploads/products/b.jpg"], product.ImageUrls);
    }

    [Fact]
    public void SetImages_WithEmptyList_ClearsGallery()
    {
        var product = CreateProduct();
        product.SetImages(["/api/uploads/products/a.jpg"]);

        product.SetImages([]);

        Assert.Empty(product.ImageUrls);
    }

    [Fact]
    public void SetImages_SkipsBlankEntries()
    {
        var product = CreateProduct();

        product.SetImages(["/api/uploads/products/a.jpg", " ", "", "/api/uploads/products/b.jpg"]);

        Assert.Equal(["/api/uploads/products/a.jpg", "/api/uploads/products/b.jpg"], product.ImageUrls);
    }

    [Fact]
    public void SetPromotion_Valid_MakesProductOnPromotionWithinWindow()
    {
        var product = CreateProduct();
        var start = DateTime.UtcNow.AddHours(-1);
        var end = DateTime.UtcNow.AddHours(1);

        product.SetPromotion(20m, start, end);

        Assert.True(product.IsOnPromotion);
        Assert.Equal(55.92m, product.EffectivePrice.Amount);
    }

    [Fact]
    public void SetPromotion_OutsideWindow_IsNotOnPromotionAndEffectivePriceIsRegularPrice()
    {
        var product = CreateProduct();
        var start = DateTime.UtcNow.AddDays(1);
        var end = DateTime.UtcNow.AddDays(2);

        product.SetPromotion(20m, start, end);

        Assert.False(product.IsOnPromotion);
        Assert.Equal(product.Price.Amount, product.EffectivePrice.Amount);
    }

    [Fact]
    public void SetPromotion_AllNull_ClearsExistingPromotion()
    {
        var product = CreateProduct();
        product.SetPromotion(20m, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));

        product.SetPromotion(null, null, null);

        Assert.False(product.IsOnPromotion);
        Assert.Null(product.DiscountPercentage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(-5)]
    public void SetPromotion_DiscountOutOfRange_Throws(decimal discount)
    {
        var product = CreateProduct();

        Assert.Throws<DomainException>(() =>
            product.SetPromotion(discount, DateTime.UtcNow, DateTime.UtcNow.AddDays(1)));
    }

    [Fact]
    public void SetPromotion_EndBeforeStart_Throws()
    {
        var product = CreateProduct();

        Assert.Throws<DomainException>(() =>
            product.SetPromotion(20m, DateTime.UtcNow, DateTime.UtcNow.AddHours(-1)));
    }

    [Fact]
    public void SetPromotion_MissingDates_Throws()
    {
        var product = CreateProduct();

        Assert.Throws<DomainException>(() => product.SetPromotion(20m, null, null));
    }
}
