using AtelieBebe.Application.Common;

namespace AtelieBebe.Application.Orders;

public interface IOrderService
{
    Task<OrderDto> CreateStoreOrderAsync(CreateStoreOrderRequest request, Guid? customerId, CancellationToken ct = default);
    Task<OrderDto> CreateCustomOrderAsync(CreateCustomOrderRequest request, Guid? customerId, CancellationToken ct = default);
    Task<PagedResult<OrderDto>> ListAsync(string? status, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<OrderDto>> ListMineAsync(Guid customerId, CancellationToken ct = default);
    Task<OrderDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<OrderDto> ChangeStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken ct = default);
}
