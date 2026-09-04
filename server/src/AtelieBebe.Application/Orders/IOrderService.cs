namespace AtelieBebe.Application.Orders;

public interface IOrderService
{
    Task<OrderDto> CreateStoreOrderAsync(CreateStoreOrderRequest request, Guid? customerId, CancellationToken ct = default);
    Task<OrderDto> CreateCustomOrderAsync(CreateCustomOrderRequest request, Guid? customerId, CancellationToken ct = default);
    Task<IReadOnlyList<OrderDto>> ListAsync(string? status, CancellationToken ct = default);
    Task<IReadOnlyList<OrderDto>> ListMineAsync(Guid customerId, CancellationToken ct = default);
    Task<OrderDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<OrderDto> ChangeStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken ct = default);
}
