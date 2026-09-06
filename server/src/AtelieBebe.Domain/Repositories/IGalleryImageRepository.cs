using AtelieBebe.Domain.Entities;

namespace AtelieBebe.Domain.Repositories;

public interface IGalleryImageRepository
{
    Task<GalleryImage?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<GalleryImage>> ListAsync(CancellationToken ct = default);
    void Add(GalleryImage image);
    void Remove(GalleryImage image);
}
