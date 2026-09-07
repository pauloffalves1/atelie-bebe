namespace AtelieBebe.Infrastructure.Payments;

public sealed class MercadoPagoOptions
{
    public const string SectionName = "MercadoPago";

    /// <summary>Test ("TEST-...") or production access token from the Mercado Pago developer panel. Blank in appsettings.json — set via dotnet user-secrets.</summary>
    public string AccessToken { get; set; } = default!;
}
