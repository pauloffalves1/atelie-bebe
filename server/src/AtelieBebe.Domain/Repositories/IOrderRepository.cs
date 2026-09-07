using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Enums;

namespace AtelieBebe.Domain.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Order> Items, int TotalItems)> ListAsync(OrderStatus? status, PaymentStatus? paymentStatus, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> ListByCustomerAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>Used to gate product reviews to customers who actually bought the product — any order status counts, not just delivered.</summary>
    Task<bool> CustomerHasPurchasedProductAsync(Guid customerId, Guid productId, CancellationToken ct = default);

    void Add(Order order);
}
