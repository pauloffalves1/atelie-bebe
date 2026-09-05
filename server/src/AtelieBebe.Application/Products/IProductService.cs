using AtelieBebe.Application.Common;

namespace AtelieBebe.Application.Products;

public interface IProductService
{
    Task<PagedResult<ProductDto>> ListAsync(string? category, bool onlyActive, int page, int pageSize, Guid? customerId = null, CancellationToken ct = default);
    Task<IReadOnlyList<ProductDto>> ListFeaturedAsync(Guid? customerId = null, CancellationToken ct = default);
    Task<IReadOnlyList<string>> ListCategoriesAsync(Guid? customerId = null, CancellationToken ct = default);
    Task<ProductDto> GetBySlugAsync(string slug, Guid? customerId = null, CancellationToken ct = default);
    Task<ProductDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AdminProductDto> GetForAdminAsync(Guid id, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default);
    Task<ProductDto> UpdateStockAsync(Guid id, UpdateStockRequest request, CancellationToken ct = default);
    Task<ProductDto> SetActiveAsync(Guid id, bool active, CancellationToken ct = default);
    Task<AdminProductDto> SetAllowedCustomersAsync(Guid id, SetAllowedCustomersRequest request, CancellationToken ct = default);
}
