using System.Text.Json;
using AtelieBebe.Application.Abstractions;
using AtelieBebe.Application.Cart;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AtelieBebe.Infrastructure.Cart;

/// <summary>
/// Polls for cart snapshots that went quiet for a while and never turned into an order, and
/// e-mails the customer once per abandoned cart. Independent of the outbox — this isn't a
/// reaction to a domain event, it's a time-based check ("has enough silence passed?"), so it
/// runs as its own periodic background service instead of a new domain event/outbox case.
/// </summary>
public sealed class AbandonedCartReminderProcessor : BackgroundService
{
    private static readonly TimeSpan AbandonedAfter = TimeSpan.FromHours(3);
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(15);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AbandonedCartReminderProcessor> _logger;

    public AbandonedCartReminderProcessor(IServiceScopeFactory scopeFactory, ILogger<AbandonedCartReminderProcessor> logger)
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
                await ProcessAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao processar lembretes de carrinho abandonado.");
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

    private async Task ProcessAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var appUrls = scope.ServiceProvider.GetRequiredService<IAppUrlProvider>();

        var cutoff = DateTime.UtcNow - AbandonedAfter;
        var abandoned = await unitOfWork.CartSnapshots.ListAbandonedAsync(cutoff, ct);
        if (abandoned.Count == 0) return;

        var siteUrl = appUrls.PublicUrl.TrimEnd('/');

        foreach (var snapshot in abandoned)
        {
            var customer = await unitOfWork.Customers.GetByIdAsync(snapshot.CustomerId, ct);
            if (customer is null || customer.IsAnonymized)
            {
                snapshot.MarkReminderSent();
                continue;
            }

            var savedItems = JsonSerializer.Deserialize<List<CartSyncItemDto>>(snapshot.ItemsJson) ?? [];
            var emailItems = new List<AbandonedCartItem>();
            foreach (var item in savedItems)
            {
                var product = await unitOfWork.Products.GetByIdAsync(item.ProductId, ct);
                if (product is null) continue;
                emailItems.Add(new AbandonedCartItem(product.Name, $"{siteUrl}/produto/{product.Slug}", item.Quantity));
            }

            if (emailItems.Count > 0)
            {
                try
                {
                    await emailSender.SendAbandonedCartReminderAsync(customer.Name, customer.Email.Value, emailItems, $"{siteUrl}/loja", ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao enviar lembrete de carrinho abandonado para o cliente {CustomerId}.", customer.Id);
                }
            }

            snapshot.MarkReminderSent();
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}
