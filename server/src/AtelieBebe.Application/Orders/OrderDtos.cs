namespace AtelieBebe.Application.Orders;

public sealed record OrderItemDto(
    Guid Id,
    Guid? ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal Subtotal,
    string? OptionsJson);

public sealed record OrderDto(
    Guid Id,
    Guid? CustomerId,
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    string? CustomerCpf,
    string Type,
    string Status,
    decimal Total,
    string? Notes,
    string? CustomDetailsJson,
    string? ShippingAddressJson,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<OrderItemDto> Items);

public sealed record CreateOrderItemRequest(Guid? ProductId, string ProductName, decimal UnitPrice, int Quantity, string? OptionsJson);

public sealed record CreateStoreOrderRequest(
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    string CustomerCpf,
    string? Notes,
    string? ShippingAddressJson,
    IReadOnlyList<CreateOrderItemRequest> Items);

public sealed record CreateCustomOrderRequest(
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    string CustomerCpf,
    string? Notes,
    string CustomDetailsJson,
    decimal EstimatedPrice);

public sealed record UpdateOrderStatusRequest(string Status);
