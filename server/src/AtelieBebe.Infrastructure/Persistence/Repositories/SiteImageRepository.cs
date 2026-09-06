using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AtelieBebe.Infrastructure.Persistence.Repositories;

public sealed class SiteImageRepository : ISiteImageRepository
{
    private readonly AppDbContext _dbContext;

    public SiteImageRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<SiteImage?> GetByKeyAsync(string key, CancellationToken ct = default) =>
        _dbContext.SiteImages.FirstOrDefaultAsync(s => s.Key == key, ct);

    public async Task<IReadOnlyList<SiteImage>> ListAsync(CancellationToken ct = default) =>
        await _dbContext.SiteImages.OrderBy(s => s.Key).ToListAsync(ct);

    public void Add(SiteImage siteImage) => _dbContext.SiteImages.Add(siteImage);
}
