using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Exceptions;
using AtelieBebe.Domain.ValueObjects;

namespace AtelieBebe.Domain.Tests.Entities;

public class AdminTests
{
    private static readonly Email AdminEmail = Email.Create("admin@ateliebebe.com.br");

    [Fact]
    public void Create_WithEmptyName_Throws()
    {
        Assert.Throws<DomainException>(() => Admin.Create(" ", AdminEmail, "hash"));
    }

    [Fact]
    public void Create_WithEmptyPasswordHash_Throws()
    {
        Assert.Throws<DomainException>(() => Admin.Create("Admin", AdminEmail, ""));
    }

    [Fact]
    public void Create_Valid_TwoFactorDisabledByDefault()
    {
        var admin = Admin.Create("Admin", AdminEmail, "hash");

        Assert.False(admin.TwoFactorEnabled);
        Assert.Null(admin.TwoFactorSecret);
    }

    [Fact]
    public void EnableTwoFactor_WithEmptySecret_Throws()
    {
        var admin = Admin.Create("Admin", AdminEmail, "hash");

        Assert.Throws<DomainException>(() => admin.EnableTwoFactor(" "));
    }

    [Fact]
    public void EnableTwoFactor_Valid_SetsSecretAndEnables()
    {
        var admin = Admin.Create("Admin", AdminEmail, "hash");

        admin.EnableTwoFactor("SECRET123");

        Assert.True(admin.TwoFactorEnabled);
        Assert.Equal("SECRET123", admin.TwoFactorSecret);
    }

    [Fact]
    public void DisableTwoFactor_ClearsSecretAndDisables()
    {
        var admin = Admin.Create("Admin", AdminEmail, "hash");
        admin.EnableTwoFactor("SECRET123");

        admin.DisableTwoFactor();

        Assert.False(admin.TwoFactorEnabled);
        Assert.Null(admin.TwoFactorSecret);
    }
}
