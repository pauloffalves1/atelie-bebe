using AtelieBebe.Domain.Common;
using AtelieBebe.Domain.Exceptions;

namespace AtelieBebe.Domain.Entities;

/// <summary>A star rating (1-5) plus an optional comment left by a customer who bought the product.</summary>
public sealed class ProductReview : Entity, IAggregateRoot
{
    public Guid ProductId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = default!;
    public int Rating { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ProductReview() { } // EF Core

    private ProductReview(Guid id, Guid productId, Guid customerId, string customerName, int rating, string? comment) : base(id)
    {
        ProductId = productId;
        CustomerId = customerId;
        CustomerName = customerName;
        Rating = rating;
        Comment = comment;
        CreatedAt = DateTime.UtcNow;
    }

    public static ProductReview Create(Guid productId, Guid customerId, string customerName, int rating, string? comment)
    {
        if (productId == Guid.Empty)
            throw new DomainException("Produto inválido.");
        if (customerId == Guid.Empty)
            throw new DomainException("Cliente inválido.");
        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("Nome do cliente é obrigatório.");
        if (rating is < 1 or > 5)
            throw new DomainException("A nota deve ser entre 1 e 5.");

        return new ProductReview(
            Guid.NewGuid(), productId, customerId, customerName.Trim(), rating,
            string.IsNullOrWhiteSpace(comment) ? null : comment.Trim());
    }
}
