using AtelieBebe.Domain.Common;

namespace AtelieBebe.Domain.Events;

public sealed record OrderCreatedDomainEvent(
    Guid OrderId,
    string CustomerName,
    string CustomerEmail,
    decimal TotalAmount) : DomainEventBase;
