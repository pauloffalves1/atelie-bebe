using AtelieBebe.Application.Products;

namespace AtelieBebe.Application.Wishlist;

public sealed record WishlistItemDto(Guid Id, ProductDto Product, DateTime CreatedAt);

public sealed record WishlistStatusDto(bool IsFavorited);
