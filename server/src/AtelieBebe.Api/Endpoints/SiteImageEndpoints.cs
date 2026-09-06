using AtelieBebe.Application.Exceptions;
using AtelieBebe.Application.SiteImages;

namespace AtelieBebe.Api.Endpoints;

public static class SiteImageEndpoints
{
    /// <summary>Known image slots the admin can replace. Add new site-image spots here as needed.</summary>
    private static readonly HashSet<string> AllowedKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "home-hero",
        "about",
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp",
    };

    private const long MaxFileSizeBytes = 8 * 1024 * 1024; // 8 MB

    public static void MapSiteImageEndpoints(this WebApplication app)
    {
        app.MapGet("/api/site-images", async (ISiteImageService service, CancellationToken ct) =>
            Results.Ok(await service.ListAsync(ct)))
            .WithTags("Imagens do site");

        var adminGroup = app.MapGroup("/api/admin/site-images").WithTags("Imagens do site (admin)").RequireAuthorization("AdminOnly");

        adminGroup.MapPost("/{key}", async (string key, IFormFile file, Application.Abstractions.IFileStorageService fileStorage,
            ISiteImageService service, CancellationToken ct) =>
        {
            if (!AllowedKeys.Contains(key))
                throw new ConflictException($"Chave de imagem inválida: '{key}'.");

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
                throw new ConflictException("Formato de imagem não suportado. Use JPG, PNG ou WEBP.");

            if (file.Length == 0)
                throw new ConflictException("O arquivo enviado está vazio.");
            if (file.Length > MaxFileSizeBytes)
                throw new ConflictException("A imagem deve ter no máximo 8MB.");

            var fileName = $"{key}-{Guid.NewGuid():N}{extension}";
            await using var stream = file.OpenReadStream();
            var url = await fileStorage.SaveAsync("site", fileName, stream, ct);

            return Results.Ok(await service.SetImageAsync(key, url, ct));
        }).DisableAntiforgery();
    }
}
