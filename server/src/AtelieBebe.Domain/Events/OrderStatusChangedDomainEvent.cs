using AtelieBebe.Domain.Common;
using AtelieBebe.Domain.Enums;

namespace AtelieBebe.Domain.Events;

public sealed record OrderStatusChangedDomainEvent(
    Guid OrderId,
    string CustomerEmail,
    OrderStatus OldStatus,
    OrderStatus NewStatus) : DomainEventBase;
