using AtelieBebe.Application.Products;

namespace AtelieBebe.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Produtos");

        group.MapGet("/", async (string? category, IProductService service, CancellationToken ct, int page = 1, int pageSize = 12) =>
            Results.Ok(await service.ListAsync(category, onlyActive: true, page, pageSize, ct)));

        group.MapGet("/featured", async (IProductService service, CancellationToken ct) =>
            Results.Ok(await service.ListFeaturedAsync(ct)));

        group.MapGet("/categories", async (IProductService service, CancellationToken ct) =>
            Results.Ok(await service.ListCategoriesAsync(ct)));

        group.MapGet("/{slug}", async (string slug, IProductService service, CancellationToken ct) =>
            Results.Ok(await service.GetBySlugAsync(slug, ct)));

        var adminGroup = app.MapGroup("/api/admin/products").WithTags("Produtos (admin)").RequireAuthorization("AdminOnly");

        adminGroup.MapGet("/", async (IProductService service, CancellationToken ct, int page = 1, int pageSize = 20) =>
            Results.Ok(await service.ListAsync(null, onlyActive: false, page, pageSize, ct)));

        adminGroup.MapGet("/{id:guid}", async (Guid id, IProductService service, CancellationToken ct) =>
            Results.Ok(await service.GetByIdAsync(id, ct)));

        adminGroup.MapPost("/", async (CreateProductRequest request, IProductService service, CancellationToken ct) =>
        {
            var created = await service.CreateAsync(request, ct);
            return Results.Created($"/api/admin/products/{created.Id}", created);
        });

        adminGroup.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest request, IProductService service, CancellationToken ct) =>
            Results.Ok(await service.UpdateAsync(id, request, ct)));

        adminGroup.MapPatch("/{id:guid}/stock", async (Guid id, UpdateStockRequest request, IProductService service, CancellationToken ct) =>
            Results.Ok(await service.UpdateStockAsync(id, request, ct)));

        adminGroup.MapPatch("/{id:guid}/active", async (Guid id, bool active, IProductService service, CancellationToken ct) =>
            Results.Ok(await service.SetActiveAsync(id, active, ct)));
    }
}
