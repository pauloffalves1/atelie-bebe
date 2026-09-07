using AtelieBebe.Application.Abstractions;
using AtelieBebe.Application.Common;
using AtelieBebe.Application.Exceptions;
using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Enums;
using AtelieBebe.Domain.ValueObjects;

namespace AtelieBebe.Application.Orders;

public sealed class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentGateway _paymentGateway;

    public OrderService(IUnitOfWork unitOfWork, IPaymentGateway paymentGateway)
    {
        _unitOfWork = unitOfWork;
        _paymentGateway = paymentGateway;
    }

    public async Task<OrderDto> CreateStoreOrderAsync(CreateStoreOrderRequest request, Guid? customerId, CancellationToken ct = default)
    {
        if (request.Items.Count == 0)
            throw new ConflictException("O pedido precisa ter pelo menos um item.");

        var order = Order.Create(
            customerId,
            request.CustomerName,
            Email.Create(request.CustomerEmail),
            request.CustomerPhone,
            Cpf.Create(request.CustomerCpf),
            OrderType.Loja,
            request.Notes,
            customDetailsJson: null,
            request.ShippingAddressJson,
            Money.FromReais(request.ShippingCost));

        foreach (var itemRequest in request.Items)
        {
            if (itemRequest.ProductId is Guid productId)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(productId, ct)
                    ?? throw new NotFoundException("Produto", productId);

                order.AddItem(product.Id, product.Name, product.EffectivePrice, itemRequest.Quantity, itemRequest.OptionsJson);
            }
            else
            {
                order.AddItem(null, itemRequest.ProductName, Money.FromReais(itemRequest.UnitPrice), itemRequest.Quantity, itemRequest.OptionsJson);
            }
        }

        order.Submit();
        _unitOfWork.Orders.Add(order);
        await _unitOfWork.SaveChangesAsync(ct);

        var dto = ToDto(order);

        if (_paymentGateway.IsConfigured)
        {
            var preference = await _paymentGateway.CreatePreferenceAsync(
                order.Id, "Pedido Ateliê Layette Baby", order.Total.Amount, order.CustomerEmail.Value, ct);

            if (preference is not null)
                dto = dto with { PaymentUrl = preference.CheckoutUrl };
        }

        return dto;
    }

    public async Task<OrderDto> CreateCustomOrderAsync(CreateCustomOrderRequest request, Guid? customerId, CancellationToken ct = default)
    {
        var order = Order.Create(
            customerId,
            request.CustomerName,
            Email.Create(request.CustomerEmail),
            request.CustomerPhone,
            Cpf.Create(request.CustomerCpf),
            OrderType.Personalizada,
            request.Notes,
            request.CustomDetailsJson);

        order.AddItem(null, "Encomenda personalizada", Money.FromReais(request.EstimatedPrice), 1);
        order.Submit();

        _unitOfWork.Orders.Add(order);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(order.Id, ct);
    }

    public async Task<PagedResult<OrderDto>> ListAsync(string? status, string? paymentStatus, int page, int pageSize, CancellationToken ct = default)
    {
        var (parsedStatus, parsedPaymentStatus) = ParseFilters(status, paymentStatus);

        var (normalizedPage, normalizedPageSize) = Pagination.Normalize(page, pageSize);
        var (orders, totalItems) = await _unitOfWork.Orders.ListAsync(parsedStatus, parsedPaymentStatus, normalizedPage, normalizedPageSize, ct);
        return new PagedResult<OrderDto>(orders.Select(ToDto).ToList(), normalizedPage, normalizedPageSize, totalItems);
    }

    public async Task<IReadOnlyList<OrderDto>> ExportAsync(string? status, string? paymentStatus, CancellationToken ct = default)
    {
        var (parsedStatus, parsedPaymentStatus) = ParseFilters(status, paymentStatus);
        var orders = await _unitOfWork.Orders.ListAllAsync(parsedStatus, parsedPaymentStatus, ct);
        return orders.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<OrderDto>> ListMineAsync(Guid customerId, CancellationToken ct = default)
    {
        var orders = await _unitOfWork.Orders.ListByCustomerAsync(customerId, ct);
        return orders.Select(ToDto).OrderByDescending(o => o.CreatedAt).ToList();
    }

    public async Task<OrderDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Pedido", id);
        return ToDto(order);
    }

    public async Task<OrderDto> ChangeStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken ct = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Pedido", id);

        order.ChangeStatus(ParseStatus(request.Status));
        await _unitOfWork.SaveChangesAsync(ct);

        return ToDto(order);
    }

    public async Task HandlePaymentWebhookAsync(string paymentId, CancellationToken ct = default)
    {
        var details = await _paymentGateway.GetPaymentAsync(paymentId, ct);
        if (details is null || string.IsNullOrWhiteSpace(details.ExternalReference))
            return;

        if (!Guid.TryParse(details.ExternalReference, out var orderId))
            return;

        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, ct);
        if (order is null)
            return;

        switch (details.Status)
        {
            case "approved":
                order.MarkPaymentApproved(paymentId);
                break;
            case "rejected":
            case "cancelled":
                order.MarkPaymentRejected(paymentId);
                break;
            default:
                return;
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<string> GeneratePaymentLinkAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, ct)
            ?? throw new NotFoundException("Pedido", orderId);

        if (order.PaymentStatus == PaymentStatus.Pago)
            throw new ConflictException("Este pedido já está pago — não é necessário gerar um novo link de pagamento.");

        if (!_paymentGateway.IsConfigured)
            throw new ConflictException("O meio de pagamento online ainda não foi configurado.");

        var preference = await _paymentGateway.CreatePreferenceAsync(
            order.Id, "Pedido Ateliê Layette Baby", order.Total.Amount, order.CustomerEmail.Value, ct);

        if (preference is null)
            throw new ConflictException("Não foi possível gerar o link de pagamento agora. Tente novamente em instantes.");

        return preference.CheckoutUrl;
    }

    public async Task<OrderDto> SetTrackingCodeAsync(Guid orderId, SetTrackingCodeRequest request, CancellationToken ct = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, ct)
            ?? throw new NotFoundException("Pedido", orderId);

        order.SetTrackingCode(request.TrackingCode);
        await _unitOfWork.SaveChangesAsync(ct);

        return ToDto(order);
    }

    public async Task<OrderDto> SimulatePaymentAsync(Guid orderId, bool approved, CancellationToken ct = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, ct)
            ?? throw new NotFoundException("Pedido", orderId);

        if (approved)
            order.MarkPaymentApproved($"FAKE-{orderId}");
        else
            order.MarkPaymentRejected($"FAKE-{orderId}");

        await _unitOfWork.SaveChangesAsync(ct);

        return ToDto(order);
    }

    private static OrderStatus ParseStatus(string status)
    {
        if (!Enum.TryParse<OrderStatus>(status, true, out var parsed))
            throw new ConflictException($"Status de pedido inválido: '{status}'.");
        return parsed;
    }

    private static (OrderStatus? Status, PaymentStatus? PaymentStatus) ParseFilters(string? status, string? paymentStatus)
    {
        OrderStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
            parsedStatus = ParseStatus(status);

        PaymentStatus? parsedPaymentStatus = null;
        if (!string.IsNullOrWhiteSpace(paymentStatus))
        {
            if (!Enum.TryParse<PaymentStatus>(paymentStatus, true, out var parsed))
                throw new ConflictException($"Status de pagamento inválido: '{paymentStatus}'.");
            parsedPaymentStatus = parsed;
        }

        return (parsedStatus, parsedPaymentStatus);
    }

    private static OrderDto ToDto(Order o) => new(
        o.Id,
        o.CustomerId,
        o.CustomerName,
        o.CustomerEmail.Value,
        o.CustomerPhone,
        o.CustomerCpf?.Value,
        o.Type.ToString(),
        o.Status.ToString(),
        o.ItemsTotal.Amount,
        o.ShippingCost.Amount,
        o.Total.Amount,
        o.Notes,
        o.CustomDetailsJson,
        o.ShippingAddressJson,
        o.CreatedAt,
        o.UpdatedAt,
        o.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.ProductName, i.UnitPrice.Amount, i.Quantity, i.Subtotal.Amount, i.OptionsJson)).ToList(),
        o.PaymentStatus.ToString(),
        o.ExternalPaymentId,
        o.TrackingCode);
}
