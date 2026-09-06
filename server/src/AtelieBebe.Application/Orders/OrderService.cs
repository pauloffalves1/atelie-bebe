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

    public OrderService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

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

                order.AddItem(product.Id, product.Name, product.Price, itemRequest.Quantity, itemRequest.OptionsJson);
            }
            else
            {
                order.AddItem(null, itemRequest.ProductName, Money.FromReais(itemRequest.UnitPrice), itemRequest.Quantity, itemRequest.OptionsJson);
            }
        }

        order.Submit();
        _unitOfWork.Orders.Add(order);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(order.Id, ct);
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

    public async Task<PagedResult<OrderDto>> ListAsync(string? status, int page, int pageSize, CancellationToken ct = default)
    {
        OrderStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
            parsedStatus = ParseStatus(status);

        var (normalizedPage, normalizedPageSize) = Pagination.Normalize(page, pageSize);
        var (orders, totalItems) = await _unitOfWork.Orders.ListAsync(parsedStatus, normalizedPage, normalizedPageSize, ct);
        return new PagedResult<OrderDto>(orders.Select(ToDto).ToList(), normalizedPage, normalizedPageSize, totalItems);
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

    private static OrderStatus ParseStatus(string status)
    {
        if (!Enum.TryParse<OrderStatus>(status, true, out var parsed))
            throw new ConflictException($"Status de pedido inválido: '{status}'.");
        return parsed;
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
        o.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.ProductName, i.UnitPrice.Amount, i.Quantity, i.Subtotal.Amount, i.OptionsJson)).ToList());
}
