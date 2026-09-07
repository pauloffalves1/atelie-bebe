using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AtelieBebe.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AtelieBebe.Infrastructure.Notifications;

/// <summary>
/// Sends transactional e-mails through Resend (api.resend.com/emails). Independent of, and a lot
/// simpler than, the WhatsApp channel — no message templates need pre-approval, any HTML works —
/// but still requires FromEmail's domain to be verified in the Resend dashboard before send works.
/// </summary>
public sealed class ResendEmailSender : IEmailSender
{
    private static readonly IReadOnlyDictionary<string, string> StatusLabels = new Dictionary<string, string>
    {
        ["Recebido"] = "Recebido",
        ["EmProducao"] = "Em produção",
        ["Pronto"] = "Pronto",
        ["Enviado"] = "Enviado",
        ["Entregue"] = "Entregue",
        ["Cancelado"] = "Cancelado",
    };

    private readonly HttpClient _httpClient;
    private readonly ResendOptions _options;
    private readonly ILogger<ResendEmailSender> _logger;

    public ResendEmailSender(HttpClient httpClient, IOptions<ResendOptions> options, ILogger<ResendEmailSender> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public Task SendOrderCreatedAsync(Guid orderId, string customerName, string customerEmail, decimal total, CancellationToken ct = default) =>
        SendAsync(
            customerEmail,
            $"Recebemos seu pedido #{ShortId(orderId)}!",
            Wrap($"""
                <p>Olá, {customerName}!</p>
                <p>Recebemos seu pedido <strong>#{ShortId(orderId)}</strong> no valor de <strong>{FormatMoney(total)}</strong> e já estamos cuidando dele com carinho.</p>
                <p>Você pode acompanhar o status a qualquer momento na sua conta.</p>
                """),
            ct);

    public Task SendOrderStatusChangedAsync(Guid orderId, string customerName, string customerEmail, string oldStatus, string newStatus, CancellationToken ct = default) =>
        SendAsync(
            customerEmail,
            $"Pedido #{ShortId(orderId)} — {Label(newStatus)}",
            Wrap($"""
                <p>Olá, {customerName}!</p>
                <p>Seu pedido <strong>#{ShortId(orderId)}</strong> mudou de status: agora está <strong>{Label(newStatus)}</strong>.</p>
                """),
            ct);

    public Task SendWelcomeMessageAsync(Guid customerId, string name, string email, CancellationToken ct = default) =>
        SendAsync(
            email,
            "Bem-vinda(o) ao Ateliê Layette Baby!",
            Wrap($"""
                <p>Olá, {name}!</p>
                <p>Sua conta foi criada com sucesso. Que bom ter você por aqui — esperamos que ame as peças tanto quanto amamos fazê-las.</p>
                """),
            ct);

    public Task SendContactAcknowledgementAsync(Guid messageId, string name, string email, CancellationToken ct = default) =>
        SendAsync(
            email,
            "Recebemos sua mensagem",
            Wrap($"""
                <p>Olá, {name}!</p>
                <p>Recebemos sua mensagem e vamos responder em breve pelo WhatsApp ou e-mail.</p>
                """),
            ct);

    private async Task SendAsync(string toEmail, string subject, string html, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("Resend não configurado: defina Resend:ApiKey (dotnet user-secrets).");

        if (string.IsNullOrWhiteSpace(toEmail))
            throw new InvalidOperationException($"Não é possível enviar o e-mail '{subject}': nenhum endereço de destino informado.");

        var payload = new { from = _options.FromEmail, to = new[] { toEmail }, subject, html };

        using var request = new HttpRequestMessage(HttpMethod.Post, "emails") { Content = JsonContent.Create(payload) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Falha ao enviar e-mail via Resend ('{subject}', HTTP {(int)response.StatusCode}): {body}");
        }

        _logger.LogInformation("[Resend] E-mail '{Subject}' enviado para {Email}.", subject, toEmail);
    }

    private static string Wrap(string bodyHtml) => $"""
        <div style="font-family: Arial, sans-serif; color: #4a3f3a; max-width: 480px; margin: 0 auto;">
          <h2 style="color: #e8a2ad;">Ateliê Layette Baby</h2>
          {bodyHtml}
          <p style="margin-top: 2rem; font-size: 0.85rem; color: #8a7f7a;">Fraldas de ombro e boca bordadas com carinho.</p>
        </div>
        """;

    private static string ShortId(Guid id) => id.ToString()[..8];

    private static string FormatMoney(decimal amount) => "R$ " + amount.ToString("0.00", CultureInfo.GetCultureInfo("pt-BR"));

    private static string Label(string status) => StatusLabels.TryGetValue(status, out var label) ? label : status;
}
