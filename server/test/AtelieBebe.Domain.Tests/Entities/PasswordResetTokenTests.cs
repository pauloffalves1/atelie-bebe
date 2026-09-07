using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Exceptions;

namespace AtelieBebe.Domain.Tests.Entities;

public class PasswordResetTokenTests
{
    [Fact]
    public void Create_WithEmptyHash_Throws()
    {
        Assert.Throws<DomainException>(() => PasswordResetToken.Create(Guid.NewGuid(), " ", TimeSpan.FromHours(1)));
    }

    [Fact]
    public void IsValid_FreshToken_IsTrue()
    {
        var token = PasswordResetToken.Create(Guid.NewGuid(), "hash", TimeSpan.FromHours(1));

        Assert.True(token.IsValid);
    }

    [Fact]
    public void IsValid_ExpiredToken_IsFalse()
    {
        var token = PasswordResetToken.Create(Guid.NewGuid(), "hash", TimeSpan.FromSeconds(-1));

        Assert.False(token.IsValid);
    }

    [Fact]
    public void MarkUsed_Valid_SetsUsedAtAndInvalidatesToken()
    {
        var token = PasswordResetToken.Create(Guid.NewGuid(), "hash", TimeSpan.FromHours(1));

        token.MarkUsed();

        Assert.NotNull(token.UsedAt);
        Assert.False(token.IsValid);
    }

    [Fact]
    public void MarkUsed_AlreadyUsed_Throws()
    {
        var token = PasswordResetToken.Create(Guid.NewGuid(), "hash", TimeSpan.FromHours(1));
        token.MarkUsed();

        Assert.Throws<DomainException>(() => token.MarkUsed());
    }

    [Fact]
    public void MarkUsed_Expired_Throws()
    {
        var token = PasswordResetToken.Create(Guid.NewGuid(), "hash", TimeSpan.FromSeconds(-1));

        Assert.Throws<DomainException>(() => token.MarkUsed());
    }
}
