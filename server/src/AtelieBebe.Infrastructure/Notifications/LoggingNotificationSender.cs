using AtelieBebe.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace AtelieBebe.Infrastructure.Notifications;

/// <summary>
/// Stand-in notification channel: logs what would be sent. Swap for a real WhatsApp/e-mail/SMS
/// provider by implementing INotificationSender again — nothing else in the app needs to change.
/// </summary>
public sealed class LoggingNotificationSender : INotificationSender
{
    private readonly ILogger<LoggingNotificationSender> _logger;

    public LoggingNotificationSender(ILogger<LoggingNotificationSender> logger) => _logger = logger;

    public Task SendOrderCreatedAsync(Guid orderId, string customerName, string customerPhone, decimal total, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[notificação] Pedido {OrderId} recebido de {CustomerName} <{Phone}> — total R$ {Total:0.00}. Mensagem de confirmação enviada.",
            orderId, customerName, customerPhone, total);
        return Task.CompletedTask;
    }

    public Task SendOrderStatusChangedAsync(Guid orderId, string customerName, string customerPhone, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[notificação] Pedido {OrderId} mudou de '{OldStatus}' para '{NewStatus}'. Mensagem enviada para {CustomerName} <{Phone}>.",
            orderId, oldStatus, newStatus, customerName, customerPhone);
        return Task.CompletedTask;
    }

    public Task SendWelcomeMessageAsync(Guid customerId, string name, string phone, CancellationToken ct = default)
    {
        _logger.LogInformation("[notificação] Boas-vindas enviadas para {Name} <{Phone}> (cliente {CustomerId}).", name, phone, customerId);
        return Task.CompletedTask;
    }

    public Task SendContactAcknowledgementAsync(Guid messageId, string name, string phone, CancellationToken ct = default)
    {
        _logger.LogInformation("[notificação] Confirmação de recebimento de contato enviada para {Name} <{Phone}>.", name, phone);
        return Task.CompletedTask;
    }
}
