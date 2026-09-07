namespace AtelieBebe.Application.Products;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    decimal Price,
    string Category,
    string? ImageUrl,
    bool Active,
    bool Featured,
    bool IsExclusive,
    IReadOnlyList<string> ImageUrls);

public sealed record AdminProductDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    decimal Price,
    string Category,
    string? ImageUrl,
    bool Active,
    bool Featured,
    bool IsExclusive,
    IReadOnlyCollection<Guid> AllowedCustomerIds,
    IReadOnlyList<string> ImageUrls);

public sealed record SetAllowedCustomersRequest(IReadOnlyCollection<Guid> CustomerIds);

public sealed record SetProductImagesRequest(IReadOnlyList<string> ImageUrls);

public sealed record CreateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    string Category,
    string? ImageUrl,
    bool Featured);

public sealed record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    string Category,
    string? ImageUrl,
    bool Featured);
