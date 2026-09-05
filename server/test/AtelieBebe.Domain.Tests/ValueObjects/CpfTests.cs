using AtelieBebe.Domain.Exceptions;
using AtelieBebe.Domain.ValueObjects;

namespace AtelieBebe.Domain.Tests.ValueObjects;

public class CpfTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyValue_Throws(string? value)
    {
        Assert.Throws<DomainException>(() => Cpf.Create(value!));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("52998224700")]
    public void Create_WithInvalidCpf_Throws(string value)
    {
        Assert.Throws<DomainException>(() => Cpf.Create(value));
    }

    [Fact]
    public void Create_StripsFormattingAndKeepsDigits()
    {
        var cpf = Cpf.Create("529.982.247-25");

        Assert.Equal("52998224725", cpf.Value);
    }

    [Fact]
    public void Equality_IsValueBased()
    {
        var a = Cpf.Create("52998224725");
        var b = Cpf.Create("529.982.247-25");

        Assert.Equal(a, b);
    }
}
