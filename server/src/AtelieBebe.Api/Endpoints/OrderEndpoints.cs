using AtelieBebe.Api.Common;
using AtelieBebe.Application.Orders;

namespace AtelieBebe.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Encomendas");

        // Store checkout and custom-order submissions are open to guests too; when the caller
        // is an authenticated customer, the order is linked to their account automatically.
        group.MapPost("/store", async (CreateStoreOrderRequest request, HttpContext http, IOrderService service, CancellationToken ct) =>
        {
            var customerId = http.User.GetUserIdOrNull();
            return Results.Ok(await service.CreateStoreOrderAsync(request, customerId, ct));
        });

        group.MapPost("/custom", async (CreateCustomOrderRequest request, HttpContext http, IOrderService service, CancellationToken ct) =>
        {
            var customerId = http.User.GetUserIdOrNull();
            return Results.Ok(await service.CreateCustomOrderAsync(request, customerId, ct));
        });

        group.MapGet("/mine", async (HttpContext http, IOrderService service, CancellationToken ct) =>
            Results.Ok(await service.ListMineAsync(http.User.GetUserId(), ct)))
            .RequireAuthorization("CustomerOnly");

        group.MapGet("/{id:guid}", async (Guid id, IOrderService service, CancellationToken ct) =>
            Results.Ok(await service.GetByIdAsync(id, ct)));

        var adminGroup = app.MapGroup("/api/admin/orders").WithTags("Encomendas (admin)").RequireAuthorization("AdminOnly");

        adminGroup.MapGet("/", async (string? status, IOrderService service, CancellationToken ct, int page = 1, int pageSize = 20) =>
            Results.Ok(await service.ListAsync(status, page, pageSize, ct)));

        adminGroup.MapPatch("/{id:guid}/status", async (Guid id, UpdateOrderStatusRequest request, IOrderService service, CancellationToken ct) =>
            Results.Ok(await service.ChangeStatusAsync(id, request, ct)));
    }
}
