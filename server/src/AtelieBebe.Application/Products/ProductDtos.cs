namespace AtelieBebe.Application.Products;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    decimal Price,
    string Category,
    string? ImageUrl,
    int Stock,
    bool Active,
    bool Featured);

public sealed record CreateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    string Category,
    string? ImageUrl,
    int Stock,
    bool Featured);

public sealed record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    string Category,
    string? ImageUrl,
    bool Featured);

public sealed record UpdateStockRequest(int Stock);
