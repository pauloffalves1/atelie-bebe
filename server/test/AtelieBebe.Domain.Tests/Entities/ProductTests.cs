using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Events;
using AtelieBebe.Domain.Exceptions;
using AtelieBebe.Domain.ValueObjects;

namespace AtelieBebe.Domain.Tests.Entities;

public class ProductTests
{
    private static Product CreateProduct(int stock = 10) =>
        Product.Create("Body Manga Longa", "body-manga-longa", "Descrição", Money.FromReais(69.90m),
            "Bodies", imageUrl: null, stock);

    [Theory]
    [InlineData("", "slug", "categoria")]
    [InlineData("nome", "", "categoria")]
    [InlineData("nome", "slug", "")]
    public void Create_WithMissingRequiredField_Throws(string name, string slug, string category)
    {
        Assert.Throws<DomainException>(() =>
            Product.Create(name, slug, "descrição", Money.FromReais(10m), category, null, stock: 1));
    }

    [Fact]
    public void Create_WithNegativeStock_Throws()
    {
        Assert.Throws<DomainException>(() =>
            Product.Create("Body", "body", "descrição", Money.FromReais(10m), "Bodies", null, stock: -1));
    }

    [Fact]
    public void Create_IsActiveByDefault()
    {
        var product = CreateProduct();

        Assert.True(product.Active);
    }

    [Fact]
    public void Reserve_WithNonPositiveQuantity_Throws()
    {
        var product = CreateProduct();

        Assert.Throws<DomainException>(() => product.Reserve(0));
    }

    [Fact]
    public void Reserve_MoreThanAvailableStock_Throws()
    {
        var product = CreateProduct(stock: 5);

        Assert.Throws<DomainException>(() => product.Reserve(6));
    }

    [Fact]
    public void Reserve_ExactlyAvailableStock_ReducesToZero()
    {
        var product = CreateProduct(stock: 5);

        product.Reserve(5);

        Assert.Equal(0, product.Stock);
    }

    [Fact]
    public void Reserve_AboveLowStockThreshold_DoesNotRaiseEvent()
    {
        var product = CreateProduct(stock: 10);

        product.Reserve(1); // 9 left, above the threshold of 3

        Assert.DoesNotContain(product.DomainEvents, e => e is ProductLowStockDomainEvent);
    }

    [Fact]
    public void Reserve_DownToLowStockThreshold_RaisesLowStockEvent()
    {
        var product = CreateProduct(stock: 4);

        product.Reserve(1); // 3 left, at the threshold

        var raised = Assert.Single(product.DomainEvents.OfType<ProductLowStockDomainEvent>());
        Assert.Equal(product.Id, raised.ProductId);
        Assert.Equal(3, raised.RemainingStock);
    }

    [Fact]
    public void SetStock_Negative_Throws()
    {
        var product = CreateProduct();

        Assert.Throws<DomainException>(() => product.SetStock(-1));
    }

    [Fact]
    public void SetStock_AtOrBelowThreshold_RaisesLowStockEvent()
    {
        var product = CreateProduct(stock: 10);

        product.SetStock(2);

        Assert.Contains(product.DomainEvents, e => e is ProductLowStockDomainEvent);
    }

    [Fact]
    public void SetActive_TogglesFlag()
    {
        var product = CreateProduct();

        product.SetActive(false);

        Assert.False(product.Active);
    }
}
