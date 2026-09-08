using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Exceptions;

namespace AtelieBebe.Domain.Tests.Entities;

public class CartSnapshotTests
{
    [Fact]
    public void Create_WithEmptyCustomerId_Throws()
    {
        Assert.Throws<DomainException>(() => CartSnapshot.Create(Guid.Empty, "[]"));
    }

    [Fact]
    public void Create_Valid_SetsFields()
    {
        var customerId = Guid.NewGuid();

        var snapshot = CartSnapshot.Create(customerId, "[{\"productId\":\"x\"}]");

        Assert.Equal(customerId, snapshot.CustomerId);
        Assert.Equal("[{\"productId\":\"x\"}]", snapshot.ItemsJson);
        Assert.Null(snapshot.ReminderSentAt);
    }

    [Fact]
    public void ReplaceItems_UpdatesItemsAndClearsReminder()
    {
        var snapshot = CartSnapshot.Create(Guid.NewGuid(), "[]");
        snapshot.MarkReminderSent();

        snapshot.ReplaceItems("[{\"productId\":\"y\"}]");

        Assert.Equal("[{\"productId\":\"y\"}]", snapshot.ItemsJson);
        Assert.Null(snapshot.ReminderSentAt);
    }

    [Fact]
    public void MarkReminderSent_SetsTimestamp()
    {
        var snapshot = CartSnapshot.Create(Guid.NewGuid(), "[]");

        snapshot.MarkReminderSent();

        Assert.NotNull(snapshot.ReminderSentAt);
    }
}
