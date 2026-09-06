using AtelieBebe.Application.Abstractions;
using Microsoft.Extensions.Configuration;

namespace AtelieBebe.Infrastructure.Storage;

/// <summary>
/// Saves uploaded files to a local folder outside the app's publish output, so they survive a
/// redeploy (`dotnet publish` replaces the publish directory wholesale on every deploy).
/// Served back via app.UseStaticFiles under the same "Uploads:PublicPath" prefix (Program.cs).
/// </summary>
public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;
    private readonly string _publicBasePath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _rootPath = configuration["Uploads:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        _publicBasePath = configuration["Uploads:PublicPath"] ?? "/api/uploads";
    }

    public async Task<string> SaveAsync(string folder, string fileName, Stream content, CancellationToken ct = default)
    {
        var folderPath = Path.Combine(_rootPath, folder);
        Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, fileName);
        await using (var fileStream = File.Create(filePath))
        {
            await content.CopyToAsync(fileStream, ct);
        }

        return $"{_publicBasePath}/{folder}/{fileName}";
    }

    public Task DeleteAsync(string url, CancellationToken ct = default)
    {
        if (!url.StartsWith(_publicBasePath, StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        var relativePath = url[_publicBasePath.Length..].TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var filePath = Path.Combine(_rootPath, relativePath);

        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }
}
