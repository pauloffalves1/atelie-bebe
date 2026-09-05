using AtelieBebe.Api.Common;
using AtelieBebe.Application.Products;

namespace AtelieBebe.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Produtos");

        // Anonymous-friendly: no RequireAuthorization, but an authenticated customer's token
        // (when present) is used to also surface exclusive products they were granted access to.
        group.MapGet("/", async (string? category, HttpContext http, IProductService service, CancellationToken ct, int page = 1, int pageSize = 12) =>
            Results.Ok(await service.ListAsync(category, onlyActive: true, page, pageSize, http.User.GetUserIdOrNull(), ct)));

        group.MapGet("/featured", async (HttpContext http, IProductService service, CancellationToken ct) =>
            Results.Ok(await service.ListFeaturedAsync(http.User.GetUserIdOrNull(), ct)));

        group.MapGet("/categories", async (HttpContext http, IProductService service, CancellationToken ct) =>
            Results.Ok(await service.ListCategoriesAsync(http.User.GetUserIdOrNull(), ct)));

        group.MapGet("/{slug}", async (string slug, HttpContext http, IProductService service, CancellationToken ct) =>
            Results.Ok(await service.GetBySlugAsync(slug, http.User.GetUserIdOrNull(), ct)));

        var adminGroup = app.MapGroup("/api/admin/products").WithTags("Produtos (admin)").RequireAuthorization("AdminOnly");

        adminGroup.MapGet("/", async (IProductService service, CancellationToken ct, int page = 1, int pageSize = 20) =>
            Results.Ok(await service.ListAsync(null, onlyActive: false, page, pageSize, ct: ct)));

        adminGroup.MapGet("/{id:guid}", async (Guid id, IProductService service, CancellationToken ct) =>
            Results.Ok(await service.GetForAdminAsync(id, ct)));

        adminGroup.MapPost("/", async (CreateProductRequest request, IProductService service, CancellationToken ct) =>
        {
            var created = await service.CreateAsync(request, ct);
            return Results.Created($"/api/admin/products/{created.Id}", created);
        });

        adminGroup.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest request, IProductService service, CancellationToken ct) =>
            Results.Ok(await service.UpdateAsync(id, request, ct)));

        adminGroup.MapPatch("/{id:guid}/active", async (Guid id, bool active, IProductService service, CancellationToken ct) =>
            Results.Ok(await service.SetActiveAsync(id, active, ct)));

        adminGroup.MapPut("/{id:guid}/customers", async (Guid id, SetAllowedCustomersRequest request, IProductService service, CancellationToken ct) =>
            Results.Ok(await service.SetAllowedCustomersAsync(id, request, ct)));
    }
}
