using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Exceptions;

namespace AtelieBebe.Domain.Tests.Entities;

public class GalleryImageTests
{
    [Fact]
    public void Create_WithValidUrl_Succeeds()
    {
        var image = GalleryImage.Create("/api/uploads/gallery/abc.jpg");

        Assert.Equal("/api/uploads/gallery/abc.jpg", image.Url);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyUrl_Throws(string? url)
    {
        Assert.Throws<DomainException>(() => GalleryImage.Create(url!));
    }
}
