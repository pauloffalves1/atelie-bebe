using AtelieBebe.Domain.Entities;
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
}
