namespace AtelieBebe.Application.Abstractions;

/// <summary>
/// Independent notification channel alongside INotificationSender (WhatsApp) — dispatched
/// separately by the outbox processor so a failure/misconfiguration in one channel never blocks
/// the other. Implementations should throw when not configured, same as WhatsAppNotificationSender;
/// the caller is responsible for catching and logging rather than letting it fail the whole batch.
/// </summary>
public interface IEmailSender
{
    Task SendOrderCreatedAsync(Guid orderId, string customerName, string customerEmail, decimal total, CancellationToken ct = default);
    Task SendOrderStatusChangedAsync(Guid orderId, string customerName, string customerEmail, string oldStatus, string newStatus, CancellationToken ct = default);
    Task SendWelcomeMessageAsync(Guid customerId, string name, string email, CancellationToken ct = default);
    Task SendContactAcknowledgementAsync(Guid messageId, string name, string email, CancellationToken ct = default);
}
