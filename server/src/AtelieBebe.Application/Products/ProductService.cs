using AtelieBebe.Application.Abstractions;
using AtelieBebe.Application.Common;
using AtelieBebe.Application.Exceptions;
using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.ValueObjects;

namespace AtelieBebe.Application.Products;

public sealed class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<ProductDto>> ListAsync(string? category, bool onlyActive, CancellationToken ct = default)
    {
        var products = await _unitOfWork.Products.ListAsync(category, onlyActive, ct);
        return products.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<ProductDto>> ListFeaturedAsync(CancellationToken ct = default)
    {
        var products = await _unitOfWork.Products.ListFeaturedAsync(ct);
        return products.Select(ToDto).ToList();
    }

    public Task<IReadOnlyList<string>> ListCategoriesAsync(CancellationToken ct = default) =>
        _unitOfWork.Products.ListCategoriesAsync(ct);

    public async Task<ProductDto> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var product = await _unitOfWork.Products.GetBySlugAsync(slug, ct)
            ?? throw new NotFoundException("Produto", slug);
        return ToDto(product);
    }

    public async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Produto", id);
        return ToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        var slug = SlugHelper.Slugify(request.Name);
        var existing = await _unitOfWork.Products.GetBySlugAsync(slug, ct);
        if (existing is not null)
            slug = $"{slug}-{Guid.NewGuid().ToString()[..6]}";

        var product = Product.Create(
            request.Name,
            slug,
            request.Description,
            Money.FromReais(request.Price),
            request.Category,
            request.ImageUrl,
            request.Stock,
            request.Featured);

        _unitOfWork.Products.Add(product);
        await _unitOfWork.SaveChangesAsync(ct);
        return ToDto(product);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Produto", id);

        product.UpdateDetails(
            request.Name,
            request.Description,
            Money.FromReais(request.Price),
            request.Category,
            request.ImageUrl,
            request.Featured);

        await _unitOfWork.SaveChangesAsync(ct);
        return ToDto(product);
    }

    public async Task<ProductDto> UpdateStockAsync(Guid id, UpdateStockRequest request, CancellationToken ct = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Produto", id);

        product.SetStock(request.Stock);
        await _unitOfWork.SaveChangesAsync(ct);
        return ToDto(product);
    }

    public async Task<ProductDto> SetActiveAsync(Guid id, bool active, CancellationToken ct = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Produto", id);

        product.SetActive(active);
        await _unitOfWork.SaveChangesAsync(ct);
        return ToDto(product);
    }

    private static ProductDto ToDto(Product p) => new(
        p.Id, p.Name, p.Slug, p.Description, p.Price.Amount, p.Category, p.ImageUrl, p.Stock, p.Active, p.Featured);
}
