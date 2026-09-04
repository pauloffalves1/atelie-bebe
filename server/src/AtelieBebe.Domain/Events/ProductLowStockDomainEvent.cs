using AtelieBebe.Domain.Common;

namespace AtelieBebe.Domain.Events;

public sealed record ProductLowStockDomainEvent(
    Guid ProductId,
    string ProductName,
    int RemainingStock) : DomainEventBase;
