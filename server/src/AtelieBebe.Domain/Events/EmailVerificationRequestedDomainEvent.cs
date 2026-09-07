using AtelieBebe.Domain.Common;

namespace AtelieBebe.Domain.Events;

public sealed record EmailVerificationRequestedDomainEvent(
    Guid CustomerId,
    string Name,
    string Email,
    string VerificationUrl) : DomainEventBase;
