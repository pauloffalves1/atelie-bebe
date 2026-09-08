using AtelieBebe.Api.Common;
using AtelieBebe.Application.Abstractions;
using AtelieBebe.Application.Audit;
using AtelieBebe.Application.Products;

namespace AtelieBebe.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Produtos");

        // Anonymous-friendly: no RequireAuthorization, but an authenticated customer's token
        // (when present) is used to also surface exclusive products they were granted access to.
        group.MapGet("/", async (string? category, string? search, HttpContext http, IProductService service, CancellationToken ct, int page = 1, int pageSize = 12) =>
            Results.Ok(await service.ListAsync(category, onlyActive: true, page, pageSize, http.User.GetUserIdOrNull(), search, ct)));

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

        adminGroup.MapPost("/", async (CreateProductRequest request, HttpContext http, IProductService service, IAuditLogService auditLog, CancellationToken ct) =>
        {
            var created = await service.CreateAsync(request, ct);
            await auditLog.RecordAsync(http.User.GetUserId(), http.User.GetName(), "ProductCreated", $"Produto '{created.Name}' criado", ct);
            return Results.Created($"/api/admin/products/{created.Id}", created);
        });

        adminGroup.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest request, HttpContext http, IProductService service, IAuditLogService auditLog, CancellationToken ct) =>
        {
            var updated = await service.UpdateAsync(id, request, ct);
            await auditLog.RecordAsync(http.User.GetUserId(), http.User.GetName(), "ProductUpdated", $"Produto '{updated.Name}' atualizado", ct);
            return Results.Ok(updated);
        });

        adminGroup.MapPatch("/{id:guid}/active", async (Guid id, bool active, HttpContext http, IProductService service, IAuditLogService auditLog, CancellationToken ct) =>
        {
            var updated = await service.SetActiveAsync(id, active, ct);
            await auditLog.RecordAsync(http.User.GetUserId(), http.User.GetName(), "ProductActiveChanged", $"Produto '{updated.Name}' marcado como {(active ? "ativo" : "inativo")}", ct);
            return Results.Ok(updated);
        });

        adminGroup.MapPut("/{id:guid}/customers", async (Guid id, SetAllowedCustomersRequest request, IProductService service, CancellationToken ct) =>
            Results.Ok(await service.SetAllowedCustomersAsync(id, request, ct)));

        adminGroup.MapPut("/{id:guid}/images", async (Guid id, SetProductImagesRequest request, IProductService service, CancellationToken ct) =>
            Results.Ok(await service.SetImagesAsync(id, request, ct)));

        adminGroup.MapPatch("/{id:guid}/promotion", async (Guid id, SetPromotionRequest request, HttpContext http, IProductService service, IAuditLogService auditLog, CancellationToken ct) =>
        {
            var updated = await service.SetPromotionAsync(id, request, ct);
            var summary = request.DiscountPercentage is null ? $"Promoção removida de '{updated.Name}'" : $"Promoção de {request.DiscountPercentage}% aplicada a '{updated.Name}'";
            await auditLog.RecordAsync(http.User.GetUserId(), http.User.GetName(), "ProductPromotionChanged", summary, ct);
            return Results.Ok(updated);
        });

        adminGroup.MapPost("/promotions/bulk", async (BulkApplyPromotionRequest request, HttpContext http, IProductService service, IAuditLogService auditLog, CancellationToken ct) =>
        {
            var updated = await service.ApplyPromotionToManyAsync(request, ct);
            await auditLog.RecordAsync(http.User.GetUserId(), http.User.GetName(), "ProductPromotionChanged", $"Promoção de {request.DiscountPercentage}% aplicada em massa a {updated.Count} produto(s)", ct);
            return Results.Ok(updated);
        });

        adminGroup.MapPost("/uploads", async (IFormFile file, IFileStorageService fileStorage, CancellationToken ct) =>
        {
            var extension = ImageUploadValidator.ValidateAndGetExtension(file);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            await using var stream = file.OpenReadStream();
            var url = await fileStorage.SaveAsync("products", fileName, stream, ct);

            return Results.Ok(new { url });
        }).DisableAntiforgery();
    }
}
