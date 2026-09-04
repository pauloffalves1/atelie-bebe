using AtelieBebe.Domain.Common;
using AtelieBebe.Domain.Enums;
using AtelieBebe.Domain.Events;
using AtelieBebe.Domain.Exceptions;
using AtelieBebe.Domain.ValueObjects;

namespace AtelieBebe.Domain.Entities;

public sealed class Order : Entity, IAggregateRoot
{
    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        [OrderStatus.Recebido] = new[] { OrderStatus.EmProducao, OrderStatus.Cancelado },
        [OrderStatus.EmProducao] = new[] { OrderStatus.Pronto, OrderStatus.Cancelado },
        [OrderStatus.Pronto] = new[] { OrderStatus.Enviado, OrderStatus.Cancelado },
        [OrderStatus.Enviado] = new[] { OrderStatus.Entregue },
        [OrderStatus.Entregue] = Array.Empty<OrderStatus>(),
        [OrderStatus.Cancelado] = Array.Empty<OrderStatus>(),
    };

    public Guid? CustomerId { get; private set; }
    public string CustomerName { get; private set; } = default!;
    public Email CustomerEmail { get; private set; } = default!;
    public string? CustomerPhone { get; private set; }
    public OrderType Type { get; private set; }
    public OrderStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public string? CustomDetailsJson { get; private set; }
    public string? ShippingAddressJson { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Money Total => _items.Aggregate(Money.Zero(), (acc, item) => acc.Add(item.Subtotal));

    private Order() { } // EF Core

    private Order(Guid id, Guid? customerId, string customerName, Email customerEmail, string? customerPhone,
        OrderType type, string? notes, string? customDetailsJson, string? shippingAddressJson) : base(id)
    {
        CustomerId = customerId;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        CustomerPhone = customerPhone;
        Type = type;
        Status = OrderStatus.Recebido;
        Notes = notes;
        CustomDetailsJson = customDetailsJson;
        ShippingAddressJson = shippingAddressJson;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public static Order Create(Guid? customerId, string customerName, Email customerEmail, string? customerPhone,
        OrderType type, string? notes = null, string? customDetailsJson = null, string? shippingAddressJson = null)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("O nome do cliente é obrigatório.");

        return new Order(Guid.NewGuid(), customerId, customerName.Trim(), customerEmail, customerPhone,
            type, notes, customDetailsJson, shippingAddressJson);
    }

    public void AddItem(Guid? productId, string productName, Money unitPrice, int quantity, string? optionsJson = null)
    {
        if (Status != OrderStatus.Recebido)
            throw new DomainException("Não é possível alterar itens de um pedido que já está em processamento.");

        var item = new OrderItem(productId, productName, unitPrice, quantity, optionsJson);
        item.AttachToOrder(Id);
        _items.Add(item);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Confirms the order after all items were added, raising the creation event with the final total.</summary>
    public void Submit()
    {
        if (_items.Count == 0 && Type == OrderType.Loja)
            throw new DomainException("O pedido precisa ter pelo menos um item.");

        AddDomainEvent(new OrderCreatedDomainEvent(Id, CustomerName, CustomerEmail.Value, Total.Amount));
    }

    public void ChangeStatus(OrderStatus newStatus)
    {
        if (Status == newStatus) return;

        if (!AllowedTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
            throw new DomainException($"Não é possível mudar o status de '{Status}' para '{newStatus}'.");

        var oldStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new OrderStatusChangedDomainEvent(Id, CustomerEmail.Value, oldStatus, newStatus));
    }
}
