using AtelieBebe.Api.Common;
using AtelieBebe.Application.Audit;
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
            Results.Ok(await service.LoginAsync(request, ct)))
            .RequireRateLimiting("auth");

        customerGroup.MapGet("/me", async (HttpContext http, ICustomerAuthService service, CancellationToken ct) =>
            Results.Ok(await service.GetProfileAsync(http.User.GetUserId(), ct)))
            .RequireAuthorization("CustomerOnly");

        customerGroup.MapPost("/forgot-password", async (ForgotPasswordRequest request, ICustomerAuthService service, CancellationToken ct) =>
        {
            await service.RequestPasswordResetAsync(request.Email, ct);
            return Results.NoContent();
        });

        customerGroup.MapPost("/reset-password", async (ResetPasswordRequest request, ICustomerAuthService service, CancellationToken ct) =>
        {
            await service.ResetPasswordAsync(request.Token, request.NewPassword, ct);
            return Results.NoContent();
        }).RequireRateLimiting("auth");

        customerGroup.MapPost("/delete-account", async (DeleteAccountRequest request, HttpContext http, ICustomerAuthService service, CancellationToken ct) =>
        {
            await service.DeleteAccountAsync(http.User.GetUserId(), request.Password, ct);
            return Results.NoContent();
        }).RequireAuthorization("CustomerOnly").RequireRateLimiting("auth");

        customerGroup.MapPost("/verify-email", async (VerifyEmailRequest request, ICustomerAuthService service, CancellationToken ct) =>
        {
            await service.VerifyEmailAsync(request.Token, ct);
            return Results.NoContent();
        }).RequireRateLimiting("auth");

        customerGroup.MapPost("/resend-verification", async (HttpContext http, ICustomerAuthService service, CancellationToken ct) =>
        {
            await service.ResendEmailVerificationAsync(http.User.GetUserId(), ct);
            return Results.NoContent();
        }).RequireAuthorization("CustomerOnly").RequireRateLimiting("auth");

        var adminGroup = app.MapGroup("/api/admin/auth").WithTags("Autenticação (admin)");

        adminGroup.MapPost("/login", async (AdminLoginRequest request, IAdminAuthService service, IAuditLogService auditLog, CancellationToken ct) =>
        {
            var result = await service.LoginAsync(request, ct);
            if (result.Auth is not null)
                await auditLog.RecordAsync(result.Auth.Id, result.Auth.Name, "AdminLogin", "Login administrativo", ct);
            return Results.Ok(result);
        })
        .RequireRateLimiting("auth");

        adminGroup.MapPost("/2fa/verify", async (VerifyAdminTwoFactorRequest request, IAdminAuthService service, IAuditLogService auditLog, CancellationToken ct) =>
        {
            var auth = await service.VerifyTwoFactorAsync(request, ct);
            await auditLog.RecordAsync(auth.Id, auth.Name, "AdminLogin", "Login administrativo (2FA)", ct);
            return Results.Ok(auth);
        })
        .RequireRateLimiting("auth");

        adminGroup.MapGet("/2fa/status", async (HttpContext http, IAdminAuthService service, CancellationToken ct) =>
            Results.Ok(new { enabled = await service.IsTwoFactorEnabledAsync(http.User.GetUserId(), ct) }))
            .RequireAuthorization("AdminOnly");

        adminGroup.MapPost("/2fa/setup", async (HttpContext http, IAdminAuthService service, CancellationToken ct) =>
            Results.Ok(await service.BeginTwoFactorSetupAsync(http.User.GetUserId(), ct)))
            .RequireAuthorization("AdminOnly");

        adminGroup.MapPost("/2fa/enable", async (EnableTwoFactorRequest request, HttpContext http, IAdminAuthService service, IAuditLogService auditLog, CancellationToken ct) =>
        {
            await service.EnableTwoFactorAsync(http.User.GetUserId(), request, ct);
            await auditLog.RecordAsync(http.User.GetUserId(), http.User.GetName(), "AdminTwoFactorEnabled", "Autenticação de dois fatores ativada", ct);
            return Results.NoContent();
        })
        .RequireAuthorization("AdminOnly");

        adminGroup.MapPost("/2fa/disable", async (DisableTwoFactorRequest request, HttpContext http, IAdminAuthService service, IAuditLogService auditLog, CancellationToken ct) =>
        {
            await service.DisableTwoFactorAsync(http.User.GetUserId(), request, ct);
            await auditLog.RecordAsync(http.User.GetUserId(), http.User.GetName(), "AdminTwoFactorDisabled", "Autenticação de dois fatores desativada", ct);
            return Results.NoContent();
        })
        .RequireAuthorization("AdminOnly");
    }
}
