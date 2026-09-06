using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Exceptions;

namespace AtelieBebe.Domain.Tests.Entities;

public class SiteImageTests
{
    [Fact]
    public void Create_WithValidKeyAndUrl_Succeeds()
    {
        var image = SiteImage.Create("home-hero", "/api/uploads/site/home-hero-abc.jpg");

        Assert.Equal("home-hero", image.Key);
        Assert.Equal("/api/uploads/site/home-hero-abc.jpg", image.Url);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyKey_Throws(string? key)
    {
        Assert.Throws<DomainException>(() => SiteImage.Create(key!, "/api/uploads/site/x.jpg"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyUrl_Throws(string? url)
    {
        Assert.Throws<DomainException>(() => SiteImage.Create("home-hero", url!));
    }

    [Fact]
    public void UpdateUrl_WithValidUrl_ReplacesUrlAndBumpsUpdatedAt()
    {
        var image = SiteImage.Create("about", "/api/uploads/site/about-old.jpg");
        var originalUpdatedAt = image.UpdatedAt;

        image.UpdateUrl("/api/uploads/site/about-new.jpg");

        Assert.Equal("/api/uploads/site/about-new.jpg", image.Url);
        Assert.True(image.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void UpdateUrl_WithEmptyUrl_Throws()
    {
        var image = SiteImage.Create("about", "/api/uploads/site/about-old.jpg");

        Assert.Throws<DomainException>(() => image.UpdateUrl(" "));
    }
}
