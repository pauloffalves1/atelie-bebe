using AtelieBebe.Application.Orders;

namespace AtelieBebe.Api.Endpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/payments").WithTags("Pagamentos");

        // Mercado Pago pings this URL whenever a payment's status changes, carrying only the
        // payment id — never its status, which must always be re-fetched from their API (see
        // MercadoPagoGateway). Sends the id as a query string (?data.id=...&type=payment, current
        // webhooks) or, for older integrations, in the JSON body — check both. Always returns 200
        // (even for a ping we can't make sense of) so Mercado Pago doesn't keep retrying it.
        group.MapPost("/mercadopago/webhook", async (HttpContext http, IOrderService service, CancellationToken ct) =>
        {
            var paymentId = http.Request.Query["data.id"].FirstOrDefault()
                ?? http.Request.Query["id"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(paymentId))
            {
                try
                {
                    var body = await http.Request.ReadFromJsonAsync<MercadoPagoWebhookPayload>(ct);
                    paymentId = body?.Data?.Id;
                }
                catch
                {
                    // Malformed/empty body — nothing to process, just acknowledge.
                }
            }

            if (!string.IsNullOrWhiteSpace(paymentId))
                await service.HandlePaymentWebhookAsync(paymentId, ct);

            return Results.Ok();
        }).DisableAntiforgery();
    }

    private sealed record MercadoPagoWebhookPayload(MercadoPagoWebhookData? Data);
    private sealed record MercadoPagoWebhookData(string? Id);
}
