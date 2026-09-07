using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Enums;
using AtelieBebe.Domain.Events;
using AtelieBebe.Domain.Exceptions;
using AtelieBebe.Domain.ValueObjects;

namespace AtelieBebe.Domain.Tests.Entities;

public class OrderTests
{
    private static readonly Email CustomerEmail = Email.Create("cliente@ateliebebe.com.br");
    private static readonly Cpf CustomerCpf = Cpf.Create("529.982.247-25");

    private static Order CreateStoreOrder() =>
        Order.Create(customerId: null, "Maria Silva", CustomerEmail, "11999999999", CustomerCpf, OrderType.Loja);

    private static Order CreateCustomOrder() =>
        Order.Create(customerId: null, "Maria Silva", CustomerEmail, "11999999999", CustomerCpf, OrderType.Personalizada);

    [Fact]
    public void Create_WithEmptyCustomerName_Throws()
    {
        Assert.Throws<DomainException>(() =>
            Order.Create(null, " ", CustomerEmail, "11999999999", CustomerCpf, OrderType.Loja));
    }

    [Fact]
    public void Create_WithEmptyPhone_Throws()
    {
        Assert.Throws<DomainException>(() =>
            Order.Create(null, "Maria Silva", CustomerEmail, null, CustomerCpf, OrderType.Loja));
    }

    [Fact]
    public void Create_WithoutCpf_Throws()
    {
        Assert.Throws<DomainException>(() =>
            Order.Create(null, "Maria Silva", CustomerEmail, "11999999999", null, OrderType.Loja));
    }

    [Fact]
    public void Create_SetsInitialStatusToRecebido()
    {
        var order = CreateStoreOrder();

        Assert.Equal(OrderStatus.Recebido, order.Status);
    }

    [Fact]
    public void Submit_StoreOrderWithNoItems_Throws()
    {
        var order = CreateStoreOrder();

        Assert.Throws<DomainException>(() => order.Submit());
    }

    [Fact]
    public void Submit_CustomOrderWithNoItems_DoesNotThrow()
    {
        var order = CreateCustomOrder();

        order.Submit();

        Assert.Contains(order.DomainEvents, e => e is OrderCreatedDomainEvent);
    }

    [Fact]
    public void Submit_StoreOrderWithItems_RaisesOrderCreatedEventWithTotal()
    {
        var order = CreateStoreOrder();
        order.AddItem(Guid.NewGuid(), "Body Manga Longa", Money.FromReais(69.90m), 2);

        order.Submit();

        var raised = Assert.Single(order.DomainEvents.OfType<OrderCreatedDomainEvent>());
        Assert.Equal(order.Id, raised.OrderId);
        Assert.Equal(139.80m, raised.TotalAmount);
        Assert.Equal("11999999999", raised.CustomerPhone);
    }

    [Fact]
    public void AddItem_AfterOrderLeftRecebidoStatus_Throws()
    {
        var order = CreateStoreOrder();
        order.AddItem(Guid.NewGuid(), "Body Manga Longa", Money.FromReais(69.90m), 1);
        order.ChangeStatus(OrderStatus.EmProducao);

        Assert.Throws<DomainException>(() =>
            order.AddItem(Guid.NewGuid(), "Outro item", Money.FromReais(10m), 1));
    }

    [Fact]
    public void Total_SumsAllItemSubtotals()
    {
        var order = CreateStoreOrder();
        order.AddItem(Guid.NewGuid(), "Item A", Money.FromReais(10m), 2); // 20
        order.AddItem(Guid.NewGuid(), "Item B", Money.FromReais(5m), 3);  // 15

        Assert.Equal(35m, order.Total.Amount);
    }

    [Fact]
    public void Total_WithoutShippingCost_DefaultsToZero()
    {
        var order = CreateStoreOrder();

        Assert.Equal(0m, order.ShippingCost.Amount);
    }

    [Fact]
    public void Total_IncludesShippingCostOnTopOfItemsTotal()
    {
        var order = Order.Create(null, "Maria Silva", CustomerEmail, "11999999999", CustomerCpf, OrderType.Loja,
            shippingCost: Money.FromReais(15.90m));
        order.AddItem(Guid.NewGuid(), "Item A", Money.FromReais(10m), 2); // 20

        Assert.Equal(20m, order.ItemsTotal.Amount);
        Assert.Equal(35.90m, order.Total.Amount);
    }

    [Theory]
    [InlineData(OrderStatus.Recebido, OrderStatus.EmProducao)]
    [InlineData(OrderStatus.Recebido, OrderStatus.Cancelado)]
    [InlineData(OrderStatus.EmProducao, OrderStatus.Pronto)]
    [InlineData(OrderStatus.Pronto, OrderStatus.Enviado)]
    [InlineData(OrderStatus.Enviado, OrderStatus.Entregue)]
    public void ChangeStatus_AllowedTransition_Succeeds(OrderStatus from, OrderStatus to)
    {
        var order = CreateStoreOrder();
        if (from != OrderStatus.Recebido)
            SetStatus(order, from);

        order.ChangeStatus(to);

        Assert.Equal(to, order.Status);
    }

    [Theory]
    [InlineData(OrderStatus.Recebido, OrderStatus.Pronto)]
    [InlineData(OrderStatus.Recebido, OrderStatus.Enviado)]
    [InlineData(OrderStatus.Recebido, OrderStatus.Entregue)]
    [InlineData(OrderStatus.EmProducao, OrderStatus.Enviado)]
    [InlineData(OrderStatus.Enviado, OrderStatus.Cancelado)]
    [InlineData(OrderStatus.Entregue, OrderStatus.Cancelado)]
    [InlineData(OrderStatus.Cancelado, OrderStatus.Recebido)]
    public void ChangeStatus_DisallowedTransition_Throws(OrderStatus from, OrderStatus to)
    {
        var order = CreateStoreOrder();
        if (from != OrderStatus.Recebido)
            SetStatus(order, from);

        Assert.Throws<DomainException>(() => order.ChangeStatus(to));
    }

    [Fact]
    public void ChangeStatus_ToSameStatus_IsNoOpAndRaisesNoEvent()
    {
        var order = CreateStoreOrder();

        order.ChangeStatus(OrderStatus.Recebido);

        Assert.DoesNotContain(order.DomainEvents, e => e is OrderStatusChangedDomainEvent);
    }

    [Fact]
    public void ChangeStatus_ValidTransition_RaisesStatusChangedEvent()
    {
        var order = CreateStoreOrder();

        order.ChangeStatus(OrderStatus.EmProducao);

        var raised = Assert.Single(order.DomainEvents.OfType<OrderStatusChangedDomainEvent>());
        Assert.Equal(OrderStatus.Recebido, raised.OldStatus);
        Assert.Equal(OrderStatus.EmProducao, raised.NewStatus);
        Assert.Equal("11999999999", raised.CustomerPhone);
    }

    [Fact]
    public void MarkPaymentApproved_SetsStatusAndExternalId()
    {
        var order = CreateStoreOrder();

        order.MarkPaymentApproved("mp-123");

        Assert.Equal(PaymentStatus.Pago, order.PaymentStatus);
        Assert.Equal("mp-123", order.ExternalPaymentId);
    }

    [Fact]
    public void MarkPaymentApproved_AfterAlreadyApproved_IsIdempotentAndKeepsFirstExternalId()
    {
        var order = CreateStoreOrder();
        order.MarkPaymentApproved("mp-first");

        order.MarkPaymentApproved("mp-second");

        Assert.Equal(PaymentStatus.Pago, order.PaymentStatus);
        Assert.Equal("mp-first", order.ExternalPaymentId);
    }

    [Fact]
    public void MarkPaymentRejected_SetsStatusAndExternalId()
    {
        var order = CreateStoreOrder();

        order.MarkPaymentRejected("mp-123");

        Assert.Equal(PaymentStatus.Recusado, order.PaymentStatus);
        Assert.Equal("mp-123", order.ExternalPaymentId);
    }

    [Fact]
    public void MarkPaymentRejected_AfterAlreadyApproved_DoesNotDowngradeStatus()
    {
        var order = CreateStoreOrder();
        order.MarkPaymentApproved("mp-approved");

        order.MarkPaymentRejected("mp-later");

        Assert.Equal(PaymentStatus.Pago, order.PaymentStatus);
        Assert.Equal("mp-approved", order.ExternalPaymentId);
    }

    /// <summary>Walks the order through the shortest valid path to reach an arbitrary status, for test setup.</summary>
    private static void SetStatus(Order order, OrderStatus target)
    {
        if (target == OrderStatus.Cancelado)
        {
            order.ChangeStatus(OrderStatus.Cancelado); // Recebido -> Cancelado is always a valid one-hop transition
            return;
        }

        var path = new[] { OrderStatus.EmProducao, OrderStatus.Pronto, OrderStatus.Enviado, OrderStatus.Entregue };
        foreach (var status in path)
        {
            order.ChangeStatus(status);
            if (status == target) return;
        }
    }

    [Fact]
    public void SetTrackingCode_Valid_SetsValue()
    {
        var order = CreateStoreOrder();

        order.SetTrackingCode("BR123456789BR");

        Assert.Equal("BR123456789BR", order.TrackingCode);
    }

    [Fact]
    public void SetTrackingCode_TrimsWhitespace()
    {
        var order = CreateStoreOrder();

        order.SetTrackingCode("  BR123456789BR  ");

        Assert.Equal("BR123456789BR", order.TrackingCode);
    }

    [Fact]
    public void SetTrackingCode_EmptyOrWhitespace_ClearsValue()
    {
        var order = CreateStoreOrder();
        order.SetTrackingCode("BR123456789BR");

        order.SetTrackingCode("   ");

        Assert.Null(order.TrackingCode);
    }

    [Fact]
    public void ApplyCoupon_Valid_ReducesTotal()
    {
        var order = CreateStoreOrder();
        order.AddItem(Guid.NewGuid(), "Body Manga Longa", Money.FromReais(100m), 1);

        order.ApplyCoupon("promo10", Money.FromReais(10m));

        Assert.Equal("PROMO10", order.CouponCode);
        Assert.Equal(10m, order.CouponDiscountAmount.Amount);
        Assert.Equal(90m, order.Total.Amount);
    }

    [Fact]
    public void ApplyCoupon_DiscountLargerThanTotal_ClampsAtZero()
    {
        var order = CreateStoreOrder();
        order.AddItem(Guid.NewGuid(), "Item barato", Money.FromReais(5m), 1);

        order.ApplyCoupon("BIGDISCOUNT", Money.FromReais(50m));

        Assert.Equal(0m, order.Total.Amount);
    }

    [Fact]
    public void ApplyCoupon_WithEmptyCode_Throws()
    {
        var order = CreateStoreOrder();

        Assert.Throws<DomainException>(() => order.ApplyCoupon(" ", Money.FromReais(10m)));
    }
}
