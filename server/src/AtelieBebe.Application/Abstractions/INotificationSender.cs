namespace AtelieBebe.Application.Abstractions;

/// <summary>
/// Side-effect boundary invoked by the outbox processor — never called directly from a use case,
/// so a slow or failing notification channel can never block or fail the request that created the event.
/// </summary>
public interface INotificationSender
{
    Task SendOrderCreatedAsync(Guid orderId, string customerName, string customerEmail, decimal total, CancellationToken ct = default);
    Task SendOrderStatusChangedAsync(Guid orderId, string customerEmail, string oldStatus, string newStatus, CancellationToken ct = default);
    Task SendWelcomeEmailAsync(Guid customerId, string name, string email, CancellationToken ct = default);
    Task SendLowStockAlertAsync(Guid productId, string productName, int remainingStock, CancellationToken ct = default);
    Task SendContactAcknowledgementAsync(Guid messageId, string name, string email, CancellationToken ct = default);
}
