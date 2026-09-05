namespace AtelieBebe.Application.Abstractions;

/// <summary>
/// Side-effect boundary invoked by the outbox processor — never called directly from a use case,
/// so a slow or failing notification channel can never block or fail the request that created the event.
/// </summary>
public interface INotificationSender
{
    Task SendOrderCreatedAsync(Guid orderId, string customerName, string customerPhone, decimal total, CancellationToken ct = default);
    Task SendOrderStatusChangedAsync(Guid orderId, string customerName, string customerPhone, string oldStatus, string newStatus, CancellationToken ct = default);
    Task SendWelcomeMessageAsync(Guid customerId, string name, string phone, CancellationToken ct = default);
    Task SendLowStockAlertAsync(Guid productId, string productName, int remainingStock, CancellationToken ct = default);
    Task SendContactAcknowledgementAsync(Guid messageId, string name, string phone, CancellationToken ct = default);
}
