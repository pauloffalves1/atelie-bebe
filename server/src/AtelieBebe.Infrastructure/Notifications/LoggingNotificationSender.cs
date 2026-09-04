using AtelieBebe.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace AtelieBebe.Infrastructure.Notifications;

/// <summary>
/// Stand-in notification channel: logs what would be sent. Swap for a real e-mail/SMS/WhatsApp
/// provider by implementing INotificationSender again — nothing else in the app needs to change.
/// </summary>
public sealed class LoggingNotificationSender : INotificationSender
{
    private readonly ILogger<LoggingNotificationSender> _logger;

    public LoggingNotificationSender(ILogger<LoggingNotificationSender> logger) => _logger = logger;

    public Task SendOrderCreatedAsync(Guid orderId, string customerName, string customerEmail, decimal total, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[notificação] Pedido {OrderId} recebido de {CustomerName} <{Email}> — total R$ {Total:0.00}. E-mail de confirmação enviado.",
            orderId, customerName, customerEmail, total);
        return Task.CompletedTask;
    }

    public Task SendOrderStatusChangedAsync(Guid orderId, string customerEmail, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[notificação] Pedido {OrderId} mudou de '{OldStatus}' para '{NewStatus}'. E-mail enviado para {Email}.",
            orderId, oldStatus, newStatus, customerEmail);
        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(Guid customerId, string name, string email, CancellationToken ct = default)
    {
        _logger.LogInformation("[notificação] Boas-vindas enviadas para {Name} <{Email}> (cliente {CustomerId}).", name, email, customerId);
        return Task.CompletedTask;
    }

    public Task SendLowStockAlertAsync(Guid productId, string productName, int remainingStock, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "[notificação] Estoque baixo: '{ProductName}' ({ProductId}) com {RemainingStock} unidades. Alerta enviado ao admin.",
            productName, productId, remainingStock);
        return Task.CompletedTask;
    }

    public Task SendContactAcknowledgementAsync(Guid messageId, string name, string email, CancellationToken ct = default)
    {
        _logger.LogInformation("[notificação] Confirmação de recebimento de contato enviada para {Name} <{Email}>.", name, email);
        return Task.CompletedTask;
    }
}
