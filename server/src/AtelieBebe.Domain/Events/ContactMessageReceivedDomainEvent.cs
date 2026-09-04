using AtelieBebe.Domain.Common;

namespace AtelieBebe.Domain.Events;

public sealed record ContactMessageReceivedDomainEvent(
    Guid MessageId,
    string Name,
    string Email) : DomainEventBase;
