using AtelieBebe.Domain.Common;

namespace AtelieBebe.Domain.Events;

public sealed record ProductBackInStockDomainEvent(
    Guid ProductId,
    string ProductName,
    string ProductSlug) : DomainEventBase;
