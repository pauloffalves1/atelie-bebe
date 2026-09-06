namespace AtelieBebe.Application.Abstractions;

/// <summary>Persists an uploaded file to disk and returns the public URL it can be served from.</summary>
public interface IFileStorageService
{
    Task<string> SaveAsync(string folder, string fileName, Stream content, CancellationToken ct = default);
}
