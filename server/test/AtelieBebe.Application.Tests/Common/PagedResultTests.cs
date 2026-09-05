using AtelieBebe.Application.Common;

namespace AtelieBebe.Application.Tests.Common;

public class PagedResultTests
{
    [Fact]
    public void TotalPages_IsZero_WhenThereAreNoItems()
    {
        var result = new PagedResult<string>(Items: [], Page: 1, PageSize: 20, TotalItems: 0);

        Assert.Equal(0, result.TotalPages);
    }

    [Theory]
    [InlineData(1, 20, 1)]
    [InlineData(20, 20, 1)]
    [InlineData(21, 20, 2)]
    [InlineData(40, 20, 2)]
    [InlineData(41, 20, 3)]
    public void TotalPages_RoundsUpToFitAllItems(int totalItems, int pageSize, int expectedTotalPages)
    {
        var result = new PagedResult<string>(Items: [], Page: 1, PageSize: pageSize, TotalItems: totalItems);

        Assert.Equal(expectedTotalPages, result.TotalPages);
    }

    [Fact]
    public void OutOfRangePage_WithEmptyItems_StillReportsCorrectTotals()
    {
        // Requesting a page past the end (e.g. page 99 of a 3-page result) returns no rows from the
        // repository's Skip/Take, but TotalItems/TotalPages must still reflect the whole collection.
        var result = new PagedResult<string>(Items: [], Page: 99, PageSize: 20, TotalItems: 45);

        Assert.Empty(result.Items);
        Assert.Equal(45, result.TotalItems);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void Items_AreExposedAsProvided()
    {
        var result = new PagedResult<int>(Items: [1, 2, 3], Page: 2, PageSize: 3, TotalItems: 9);

        Assert.Equal([1, 2, 3], result.Items);
        Assert.Equal(2, result.Page);
        Assert.Equal(3, result.PageSize);
    }
}
