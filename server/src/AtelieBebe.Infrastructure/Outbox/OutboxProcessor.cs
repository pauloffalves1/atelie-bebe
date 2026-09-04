using System.Text.Json;
using AtelieBebe.Application.Abstractions;
using AtelieBebe.Domain.Events;
using AtelieBebe.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AtelieBebe.Infrastructure.Outbox;

/// <summary>
/// Polls unprocessed outbox rows and dispatches each domain event to the notification sender,
/// decoupling side effects (emails/alerts) from the request thread that created them. At-least-once
/// delivery: a message only gets ProcessedOn once its handler completes without throwing.
/// </summary>
public sealed class OutboxProcessor : BackgroundService
{
    private const int BatchSize = 20;
    private const int MaxAttempts = 5;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao processar lote da outbox.");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<INotificationSender>();

        var pending = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOn == null && m.Attempts < MaxAttempts)
            .OrderBy(m => m.OccurredOn)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (pending.Count == 0) return;

        foreach (var message in pending)
        {
            try
            {
                await DispatchAsync(message, sender, ct);
                message.ProcessedOn = DateTime.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                message.Attempts += 1;
                message.Error = ex.Message;
                _logger.LogError(ex, "Falha ao processar mensagem da outbox {MessageId} (tentativa {Attempts}).", message.Id, message.Attempts);
            }
        }

        await dbContext.SaveChangesAsync(ct);
    }

    private static async Task DispatchAsync(OutboxMessage message, INotificationSender sender, CancellationToken ct)
    {
        var eventType = Type.GetType(message.Type)
            ?? throw new InvalidOperationException($"Tipo de evento desconhecido: {message.Type}");

        var domainEvent = JsonSerializer.Deserialize(message.Content, eventType)
            ?? throw new InvalidOperationException($"Não foi possível desserializar o evento {message.Type}.");

        switch (domainEvent)
        {
            case OrderCreatedDomainEvent e:
                await sender.SendOrderCreatedAsync(e.OrderId, e.CustomerName, e.CustomerEmail, e.TotalAmount, ct);
                break;
            case OrderStatusChangedDomainEvent e:
                await sender.SendOrderStatusChangedAsync(e.OrderId, e.CustomerEmail, e.OldStatus.ToString(), e.NewStatus.ToString(), ct);
                break;
            case CustomerRegisteredDomainEvent e:
                await sender.SendWelcomeEmailAsync(e.CustomerId, e.Name, e.Email, ct);
                break;
            case ProductLowStockDomainEvent e:
                await sender.SendLowStockAlertAsync(e.ProductId, e.ProductName, e.RemainingStock, ct);
                break;
            case ContactMessageReceivedDomainEvent e:
                await sender.SendContactAcknowledgementAsync(e.MessageId, e.Name, e.Email, ct);
                break;
            default:
                throw new InvalidOperationException($"Nenhum handler registrado para o evento {domainEvent.GetType().Name}.");
        }
    }
}
