using AtelieBebe.Application.Auth;
using AtelieBebe.Application.Contact;
using AtelieBebe.Application.Customers;
using AtelieBebe.Application.Orders;
using AtelieBebe.Application.Products;
using AtelieBebe.Application.SiteImages;
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
        return services;
    }
}
