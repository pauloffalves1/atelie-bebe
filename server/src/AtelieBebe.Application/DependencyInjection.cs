using AtelieBebe.Application.Audit;
using AtelieBebe.Application.Auth;
using AtelieBebe.Application.Cart;
using AtelieBebe.Application.Contact;
using AtelieBebe.Application.Newsletter;
using AtelieBebe.Application.Coupons;
using AtelieBebe.Application.Customers;
using AtelieBebe.Application.Gallery;
using AtelieBebe.Application.Orders;
using AtelieBebe.Application.Products;
using AtelieBebe.Application.Reviews;
using AtelieBebe.Application.SiteImages;
using AtelieBebe.Application.Wishlist;
using Microsoft.Extensions.DependencyInjection;

namespace AtelieBebe.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICustomerAuthService, CustomerAuthService>();
        services.AddScoped<IAdminAuthService, AdminAuthService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<ICustomerAdminService, CustomerAdminService>();
        services.AddScoped<ISiteImageService, SiteImageService>();
        services.AddScoped<IGalleryImageService, GalleryImageService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<ICartSyncService, CartSyncService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<INewsletterService, NewsletterService>();
        return services;
    }
}
