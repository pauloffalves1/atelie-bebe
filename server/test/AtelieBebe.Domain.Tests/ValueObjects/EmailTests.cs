using AtelieBebe.Domain.Exceptions;
using AtelieBebe.Domain.ValueObjects;

namespace AtelieBebe.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyValue_Throws(string? value)
    {
        Assert.Throws<DomainException>(() => Email.Create(value!));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing-domain@")]
    [InlineData("@missing-local.com")]
    [InlineData("no-at-sign.com")]
    public void Create_WithInvalidFormat_Throws(string value)
    {
        Assert.Throws<DomainException>(() => Email.Create(value));
    }

    [Fact]
    public void Create_NormalizesToTrimmedLowercase()
    {
        var email = Email.Create("  Cliente@AtelieBebe.COM.BR  ");

        Assert.Equal("cliente@ateliebebe.com.br", email.Value);
    }

    [Fact]
    public void Equality_IsValueBased()
    {
        var a = Email.Create("cliente@ateliebebe.com.br");
        var b = Email.Create("CLIENTE@ateliebebe.com.br");

        Assert.Equal(a, b);
    }
}
