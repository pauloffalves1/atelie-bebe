using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AtelieBebe.Infrastructure.Persistence.Repositories;

public sealed class GalleryImageRepository : IGalleryImageRepository
{
    private readonly AppDbContext _dbContext;

    public GalleryImageRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<GalleryImage?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _dbContext.GalleryImages.FirstOrDefaultAsync(g => g.Id == id, ct);

    public async Task<IReadOnlyList<GalleryImage>> ListAsync(CancellationToken ct = default) =>
        await _dbContext.GalleryImages.OrderByDescending(g => g.CreatedAt).ToListAsync(ct);

    public void Add(GalleryImage image) => _dbContext.GalleryImages.Add(image);

    public void Remove(GalleryImage image) => _dbContext.GalleryImages.Remove(image);
}
