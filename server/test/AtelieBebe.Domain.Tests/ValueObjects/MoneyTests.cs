using AtelieBebe.Domain.Exceptions;
using AtelieBebe.Domain.ValueObjects;

namespace AtelieBebe.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void FromReais_WithNegativeAmount_Throws()
    {
        Assert.Throws<DomainException>(() => Money.FromReais(-0.01m));
    }

    [Theory]
    [InlineData(10.005, 10.01)]
    [InlineData(10.004, 10.00)]
    [InlineData(10.995, 11.00)]
    public void FromReais_RoundsToTwoDecimalsAwayFromZero(decimal input, decimal expected)
    {
        var money = Money.FromReais(input);

        Assert.Equal(expected, money.Amount);
    }

    [Fact]
    public void Add_WithDifferentCurrencies_Throws()
    {
        var reais = Money.FromReais(10m, "BRL");
        var dollars = Money.FromReais(10m, "USD");

        Assert.Throws<DomainException>(() => reais.Add(dollars));
    }

    [Fact]
    public void Add_WithSameCurrency_SumsAmounts()
    {
        var a = Money.FromReais(10.50m);
        var b = Money.FromReais(5.25m);

        var result = a.Add(b);

        Assert.Equal(15.75m, result.Amount);
        Assert.Equal("BRL", result.Currency);
    }

    [Fact]
    public void Multiply_WithNegativeFactor_Throws()
    {
        var money = Money.FromReais(10m);

        Assert.Throws<DomainException>(() => money.Multiply(-1));
    }

    [Fact]
    public void Multiply_ComputesSubtotal()
    {
        var unitPrice = Money.FromReais(19.90m);

        var subtotal = unitPrice.Multiply(3);

        Assert.Equal(59.70m, subtotal.Amount);
    }

    [Fact]
    public void Zero_HasZeroAmountAndDefaultCurrency()
    {
        var zero = Money.Zero();

        Assert.Equal(0m, zero.Amount);
        Assert.Equal("BRL", zero.Currency);
    }

    [Fact]
    public void Equality_IsValueBased()
    {
        var a = Money.FromReais(42.00m);
        var b = Money.FromReais(42.00m);

        Assert.Equal(a, b);
        Assert.True(a.Equals(b));
    }
}
