using AtelieBebe.Domain.Entities;

namespace AtelieBebe.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<(IReadOnlyList<Product> Items, int TotalItems)> ListAsync(string? category, bool onlyActive, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> ListFeaturedAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> ListCategoriesAsync(CancellationToken ct = default);
    void Add(Product product);
    void Remove(Product product);
}
