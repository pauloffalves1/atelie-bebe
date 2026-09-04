namespace AtelieBebe.Application.Products;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> ListAsync(string? category, bool onlyActive, CancellationToken ct = default);
    Task<IReadOnlyList<ProductDto>> ListFeaturedAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> ListCategoriesAsync(CancellationToken ct = default);
    Task<ProductDto> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<ProductDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default);
    Task<ProductDto> UpdateStockAsync(Guid id, UpdateStockRequest request, CancellationToken ct = default);
    Task<ProductDto> SetActiveAsync(Guid id, bool active, CancellationToken ct = default);
}
