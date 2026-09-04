using AtelieBebe.Domain.Entities;

namespace AtelieBebe.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> ListAsync(string? category = null, bool onlyActive = true, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> ListFeaturedAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> ListCategoriesAsync(CancellationToken ct = default);
    void Add(Product product);
    void Remove(Product product);
}
