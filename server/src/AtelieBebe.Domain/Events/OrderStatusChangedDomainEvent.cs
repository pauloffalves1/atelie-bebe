using AtelieBebe.Domain.Common;
using AtelieBebe.Domain.Enums;

namespace AtelieBebe.Domain.Events;

public sealed record OrderStatusChangedDomainEvent(
    Guid OrderId,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    OrderStatus OldStatus,
    OrderStatus NewStatus) : DomainEventBase;
