using AtelieBebe.Application.Common;

namespace AtelieBebe.Application.Tests.Common;

public class PaginationTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(1, 1)]
    [InlineData(7, 7)]
    public void Normalize_ClampsPageToAtLeastOne(int requestedPage, int expectedPage)
    {
        var (page, _) = Pagination.Normalize(requestedPage, pageSize: 20);

        Assert.Equal(expectedPage, page);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(1, 1)]
    [InlineData(20, 20)]
    [InlineData(100, 100)]
    [InlineData(101, 100)]
    [InlineData(1_000_000, 100)]
    public void Normalize_ClampsPageSizeBetweenOneAndMax(int requestedPageSize, int expectedPageSize)
    {
        var (_, pageSize) = Pagination.Normalize(page: 1, requestedPageSize);

        Assert.Equal(expectedPageSize, pageSize);
    }

    [Fact]
    public void Normalize_LeavesValidInputsUnchanged()
    {
        var (page, pageSize) = Pagination.Normalize(page: 3, pageSize: 12);

        Assert.Equal(3, page);
        Assert.Equal(12, pageSize);
    }
}
