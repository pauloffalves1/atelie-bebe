using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AtelieBebe.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly AppDbContext _dbContext;

    public ProductRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        _dbContext.Products.FirstOrDefaultAsync(p => p.Slug == slug, ct);

    public async Task<(IReadOnlyList<Product> Items, int TotalItems)> ListAsync(string? category, bool onlyActive, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbContext.Products.AsQueryable();

        if (onlyActive)
            query = query.Where(p => p.Active);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        query = query.OrderBy(p => p.Name);

        var totalItems = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (items, totalItems);
    }

    public async Task<IReadOnlyList<Product>> ListFeaturedAsync(CancellationToken ct = default) =>
        await _dbContext.Products
            .Where(p => p.Active && p.Featured)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<string>> ListCategoriesAsync(CancellationToken ct = default) =>
        await _dbContext.Products
            .Where(p => p.Active)
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);

    public void Add(Product product) => _dbContext.Products.Add(product);

    public void Remove(Product product) => _dbContext.Products.Remove(product);
}
