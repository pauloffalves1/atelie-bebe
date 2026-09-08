namespace AtelieBebe.Application.Cart;

public sealed record CartSyncItemDto(Guid ProductId, int Quantity, string? EmbroideryText, string? ThreadColor);

public sealed record CartSyncRequest(IReadOnlyList<CartSyncItemDto> Items);
