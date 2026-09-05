using AtelieBebe.Domain.Common;
using AtelieBebe.Domain.Exceptions;
using AtelieBebe.Domain.ValueObjects;

namespace AtelieBebe.Domain.Entities;

public sealed class Product : Entity, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string? Description { get; private set; }
    public Money Price { get; private set; } = default!;
    public string Category { get; private set; } = default!;
    public string? ImageUrl { get; private set; }
    public bool Active { get; private set; }
    public bool Featured { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<ProductCustomerAccessEntry> _allowedCustomerAccess = new();

    /// <summary>Customers this product is restricted to. Empty means the product is public.</summary>
    public IReadOnlyCollection<Guid> AllowedCustomerIds => _allowedCustomerAccess.Select(e => e.CustomerId).ToList().AsReadOnly();

    /// <summary>A product with at least one allowed customer is exclusive — invisible to everyone else.</summary>
    public bool IsExclusive => _allowedCustomerAccess.Count > 0;

    private Product() { } // EF Core

    private Product(Guid id, string name, string slug, string? description, Money price,
        string category, string? imageUrl, bool featured) : base(id)
    {
        Name = name;
        Slug = slug;
        Description = description;
        Price = price;
        Category = category;
        ImageUrl = imageUrl;
        Active = true;
        Featured = featured;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public static Product Create(string name, string slug, string? description, Money price,
        string category, string? imageUrl, bool featured = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome do produto é obrigatório.");
        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("O slug do produto é obrigatório.");
        if (string.IsNullOrWhiteSpace(category))
            throw new DomainException("A categoria do produto é obrigatória.");

        return new Product(Guid.NewGuid(), name.Trim(), slug.Trim().ToLowerInvariant(),
            description, price, category.Trim(), imageUrl, featured);
    }

    public void UpdateDetails(string name, string? description, Money price, string category,
        string? imageUrl, bool featured)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome do produto é obrigatório.");

        Name = name.Trim();
        Description = description;
        Price = price;
        Category = category.Trim();
        ImageUrl = imageUrl;
        Featured = featured;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetActive(bool active)
    {
        Active = active;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Replaces the full set of customers allowed to see/order this product. An empty set makes it public again.</summary>
    public void SetAllowedCustomers(IEnumerable<Guid> customerIds)
    {
        _allowedCustomerAccess.Clear();
        _allowedCustomerAccess.AddRange(customerIds.Distinct().Select(id => new ProductCustomerAccessEntry(id)));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Public products are visible to everyone; exclusive products only to their allowed customers.</summary>
    public bool HasAccess(Guid? customerId) =>
        !IsExclusive || (customerId is { } id && _allowedCustomerAccess.Any(e => e.CustomerId == id));
}
