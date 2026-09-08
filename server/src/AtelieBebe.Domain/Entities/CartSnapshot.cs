using AtelieBebe.Domain.Common;
using AtelieBebe.Domain.Exceptions;

namespace AtelieBebe.Domain.Entities;

/// <summary>
/// A server-side copy of a logged-in customer's cart, pushed by the client on every change —
/// only so an abandoned-cart reminder can be sent; the cart itself still lives client-side
/// (localStorage) and this is never read back into the UI. One row per customer (upsert).
/// </summary>
public sealed class CartSnapshot : Entity, IAggregateRoot
{
    public Guid CustomerId { get; private set; }
    public string ItemsJson { get; private set; } = default!;
    public DateTime UpdatedAt { get; private set; }
    public DateTime? ReminderSentAt { get; private set; }

    private CartSnapshot() { } // EF Core

    private CartSnapshot(Guid id, Guid customerId, string itemsJson) : base(id)
    {
        CustomerId = customerId;
        ItemsJson = itemsJson;
        UpdatedAt = DateTime.UtcNow;
    }

    public static CartSnapshot Create(Guid customerId, string itemsJson)
    {
        if (customerId == Guid.Empty)
            throw new DomainException("Cliente inválido.");

        return new CartSnapshot(Guid.NewGuid(), customerId, itemsJson);
    }

    /// <summary>Replaces the item list and resets the reminder — fresh activity means it's no longer abandoned.</summary>
    public void ReplaceItems(string itemsJson)
    {
        ItemsJson = itemsJson;
        UpdatedAt = DateTime.UtcNow;
        ReminderSentAt = null;
    }

    public void MarkReminderSent() => ReminderSentAt = DateTime.UtcNow;
}
