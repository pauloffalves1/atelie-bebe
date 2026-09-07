using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AtelieBebe.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AtelieBebe.Infrastructure.Payments;

/// <summary>
/// Creates Checkout Pro preferences (a hosted payment page restricted to PIX and credit card —
/// see <c>excluded_payment_types</c> below) and resolves payment status by re-querying the
/// Mercado Pago API — webhook payloads are only a "something changed, go check" ping and are
/// never trusted for the actual status, per Mercado Pago's own integration guidance.
/// </summary>
public sealed class MercadoPagoGateway : IPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly MercadoPagoOptions _options;
    private readonly AppUrlOptions _appUrls;
    private readonly ILogger<MercadoPagoGateway> _logger;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.AccessToken);

    public MercadoPagoGateway(HttpClient httpClient, IOptions<MercadoPagoOptions> options, IOptions<AppUrlOptions> appUrls, ILogger<MercadoPagoGateway> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _appUrls = appUrls.Value;
        _logger = logger;
    }

    public async Task<PaymentPreference?> CreatePreferenceAsync(Guid orderId, string description, decimal amount, string customerEmail, CancellationToken ct = default)
    {
        if (!IsConfigured) return null;

        var orderUrl = $"{_appUrls.PublicUrl}/pedido/{orderId}";
        var payload = new
        {
            items = new[]
            {
                new { title = description, quantity = 1, unit_price = amount, currency_id = "BRL" },
            },
            payer = new { email = customerEmail },
            external_reference = orderId.ToString(),
            back_urls = new { success = orderUrl, pending = orderUrl, failure = orderUrl },
            auto_return = "approved",
            notification_url = $"{_appUrls.ApiPublicUrl}/api/payments/mercadopago/webhook",
            // Ateliê only wants to offer PIX and credit card — everything else Checkout Pro
            // would otherwise show (boleto/"ticket", debit card, prepaid card, digital wallet,
            // digital currency, ATM) is explicitly excluded rather than relying on an allow-list,
            // since Mercado Pago's API only supports narrowing via exclusion.
            payment_methods = new
            {
                excluded_payment_types = new[]
                {
                    new { id = "ticket" },
                    new { id = "debit_card" },
                    new { id = "prepaid_card" },
                    new { id = "digital_wallet" },
                    new { id = "digital_currency" },
                    new { id = "atm" },
                },
            },
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "checkout/preferences") { Content = JsonContent.Create(payload) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);

        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Falha ao criar preferência de pagamento no Mercado Pago (HTTP {Status}): {Body}", (int)response.StatusCode, body);
            return null;
        }

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        var checkoutUrl = json.GetProperty("init_point").GetString();
        if (string.IsNullOrEmpty(checkoutUrl))
        {
            _logger.LogError("Resposta do Mercado Pago sem 'init_point' ao criar preferência para o pedido {OrderId}.", orderId);
            return null;
        }

        var preferenceId = json.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
        return new PaymentPreference(checkoutUrl, preferenceId);
    }

    public async Task<PaymentDetails?> GetPaymentAsync(string paymentId, CancellationToken ct = default)
    {
        if (!IsConfigured) return null;

        using var request = new HttpRequestMessage(HttpMethod.Get, $"v1/payments/{paymentId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);

        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Falha ao consultar pagamento {PaymentId} no Mercado Pago (HTTP {Status}).", paymentId, (int)response.StatusCode);
            return null;
        }

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        var status = json.GetProperty("status").GetString();
        if (string.IsNullOrEmpty(status)) return null;

        var externalReference = json.TryGetProperty("external_reference", out var extRefEl) ? extRefEl.GetString() : null;
        return new PaymentDetails(status, externalReference);
    }
}
