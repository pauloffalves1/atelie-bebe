using AtelieBebe.Domain.Common;

namespace AtelieBebe.Domain.Events;

public sealed record CustomerRegisteredDomainEvent(
    Guid CustomerId,
    string Name,
    string Email,
    string Phone) : DomainEventBase;
