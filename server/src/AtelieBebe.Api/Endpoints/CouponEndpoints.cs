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

        adminGroup.MapPost("/", async (CreateCouponRequest request, ICouponService service, CancellationToken ct) =>
        {
            var created = await service.CreateAsync(request, ct);
            return Results.Created($"/api/admin/coupons/{created.Id}", created);
        });

        adminGroup.MapPatch("/{id:guid}/active", async (Guid id, bool active, ICouponService service, CancellationToken ct) =>
            Results.Ok(await service.SetActiveAsync(id, active, ct)));
    }
}
