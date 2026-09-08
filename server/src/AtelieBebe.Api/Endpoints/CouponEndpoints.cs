using AtelieBebe.Api.Common;
using AtelieBebe.Application.Audit;
using AtelieBebe.Application.Coupons;
using Microsoft.AspNetCore.RateLimiting;

namespace AtelieBebe.Api.Endpoints;

public static class CouponEndpoints
{
    public static void MapCouponEndpoints(this WebApplication app)
    {
        app.MapPost("/api/coupons/validate", async (ValidateCouponRequest request, ICouponService service, CancellationToken ct) =>
            Results.Ok(await service.ValidateAsync(request, ct)))
            .WithTags("Cupons")
            .RequireRateLimiting("auth");

        var adminGroup = app.MapGroup("/api/admin/coupons").WithTags("Cupons (admin)").RequireAuthorization("AdminOnly");

        adminGroup.MapGet("/", async (ICouponService service, CancellationToken ct) =>
            Results.Ok(await service.ListAsync(ct)));

        adminGroup.MapPost("/", async (CreateCouponRequest request, HttpContext http, ICouponService service, IAuditLogService auditLog, CancellationToken ct) =>
        {
            var created = await service.CreateAsync(request, ct);
            await auditLog.RecordAsync(http.User.GetUserId(), http.User.GetName(), "CouponCreated", $"Cupom '{created.Code}' criado ({created.DiscountPercentage}%)", ct);
            return Results.Created($"/api/admin/coupons/{created.Id}", created);
        });

        adminGroup.MapPatch("/{id:guid}/active", async (Guid id, bool active, HttpContext http, ICouponService service, IAuditLogService auditLog, CancellationToken ct) =>
        {
            var before = await service.GetByIdAsync(id, ct);
            var updated = await service.SetActiveAsync(id, active, ct);
            var diff = AuditDiff.Field("Status", before.Active ? "ativo" : "inativo", active ? "ativo" : "inativo") ?? "sem alterações";
            await auditLog.RecordAsync(http.User.GetUserId(), http.User.GetName(), "CouponActiveChanged", $"Cupom '{updated.Code}' — {diff}", ct);
            return Results.Ok(updated);
        });
    }
}
