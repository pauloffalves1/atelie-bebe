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

    /// <summary>Re-queries the payment gateway for the given payment id and updates the matching order's PaymentStatus. Never throws on a malformed/unknown id — webhooks must always get a 200.</summary>
    Task HandlePaymentWebhookAsync(string paymentId, CancellationToken ct = default);

    /// <summary>Development-only: sets PaymentStatus directly, bypassing the gateway entirely — backs the fake payment page used to preview the checkout flow before real Mercado Pago credentials exist.</summary>
    Task<OrderDto> SimulatePaymentAsync(Guid orderId, bool approved, CancellationToken ct = default);
}
