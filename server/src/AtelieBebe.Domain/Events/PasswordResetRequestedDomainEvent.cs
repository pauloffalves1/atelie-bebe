using AtelieBebe.Domain.Common;

namespace AtelieBebe.Domain.Events;

public sealed record PasswordResetRequestedDomainEvent(
    Guid CustomerId,
    string Name,
    string Email,
    string ResetUrl) : DomainEventBase;
