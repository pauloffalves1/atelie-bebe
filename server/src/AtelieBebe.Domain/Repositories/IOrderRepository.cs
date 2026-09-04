using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Enums;

namespace AtelieBebe.Domain.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> ListAsync(OrderStatus? status = null, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> ListByCustomerAsync(Guid customerId, CancellationToken ct = default);
    void Add(Order order);
}
