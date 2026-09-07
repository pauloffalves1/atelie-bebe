using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Exceptions;

namespace AtelieBebe.Domain.Tests.Entities;

public class CouponTests
{
    [Fact]
    public void Create_Valid_NormalizesCodeToUppercase()
    {
        var coupon = Coupon.Create(" promo10 ", 10m, null, null);

        Assert.Equal("PROMO10", coupon.Code);
        Assert.True(coupon.Active);
        Assert.Equal(0, coupon.UsesCount);
    }

    [Fact]
    public void Create_WithEmptyCode_Throws()
    {
        Assert.Throws<DomainException>(() => Coupon.Create(" ", 10m, null, null));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(-5)]
    public void Create_DiscountOutOfRange_Throws(decimal discount)
    {
        Assert.Throws<DomainException>(() => Coupon.Create("PROMO", discount, null, null));
    }

    [Fact]
    public void Create_WithZeroMaxUses_Throws()
    {
        Assert.Throws<DomainException>(() => Coupon.Create("PROMO", 10m, null, 0));
    }

    [Fact]
    public void IsValid_NoExpiryNoMaxUses_IsTrue()
    {
        var coupon = Coupon.Create("PROMO", 10m, null, null);

        Assert.True(coupon.IsValid);
    }

    [Fact]
    public void IsValid_Expired_IsFalse()
    {
        var coupon = Coupon.Create("PROMO", 10m, DateTime.UtcNow.AddDays(-1), null);

        Assert.False(coupon.IsValid);
    }

    [Fact]
    public void IsValid_Deactivated_IsFalse()
    {
        var coupon = Coupon.Create("PROMO", 10m, null, null);
        coupon.SetActive(false);

        Assert.False(coupon.IsValid);
    }

    [Fact]
    public void IsValid_MaxUsesReached_IsFalse()
    {
        var coupon = Coupon.Create("PROMO", 10m, null, 1);

        coupon.RecordUse();

        Assert.False(coupon.IsValid);
    }

    [Fact]
    public void RecordUse_Valid_IncrementsUsesCount()
    {
        var coupon = Coupon.Create("PROMO", 10m, null, 5);

        coupon.RecordUse();

        Assert.Equal(1, coupon.UsesCount);
    }

    [Fact]
    public void RecordUse_WhenInvalid_Throws()
    {
        var coupon = Coupon.Create("PROMO", 10m, DateTime.UtcNow.AddDays(-1), null);

        Assert.Throws<DomainException>(() => coupon.RecordUse());
    }
}
