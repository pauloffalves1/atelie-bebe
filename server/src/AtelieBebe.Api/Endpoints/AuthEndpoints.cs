using AtelieBebe.Api.Common;
using AtelieBebe.Application.Auth;

namespace AtelieBebe.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var customerGroup = app.MapGroup("/api/auth").WithTags("Autenticação (cliente)");

        customerGroup.MapPost("/register", async (RegisterCustomerRequest request, ICustomerAuthService service, CancellationToken ct) =>
            Results.Ok(await service.RegisterAsync(request, ct)));

        customerGroup.MapPost("/login", async (LoginRequest request, ICustomerAuthService service, CancellationToken ct) =>
            Results.Ok(await service.LoginAsync(request, ct)));

        customerGroup.MapGet("/me", async (HttpContext http, ICustomerAuthService service, CancellationToken ct) =>
            Results.Ok(await service.GetProfileAsync(http.User.GetUserId(), ct)))
            .RequireAuthorization("CustomerOnly");

        var adminGroup = app.MapGroup("/api/admin/auth").WithTags("Autenticação (admin)");

        adminGroup.MapPost("/login", async (AdminLoginRequest request, IAdminAuthService service, CancellationToken ct) =>
            Results.Ok(await service.LoginAsync(request, ct)));
    }
}
