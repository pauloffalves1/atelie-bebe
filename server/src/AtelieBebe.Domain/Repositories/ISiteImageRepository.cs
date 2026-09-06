using AtelieBebe.Domain.Entities;

namespace AtelieBebe.Domain.Repositories;

public interface ISiteImageRepository
{
    Task<SiteImage?> GetByKeyAsync(string key, CancellationToken ct = default);
    Task<IReadOnlyList<SiteImage>> ListAsync(CancellationToken ct = default);
    void Add(SiteImage siteImage);
}
