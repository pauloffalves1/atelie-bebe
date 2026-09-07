using AtelieBebe.Application.Abstractions;
using AtelieBebe.Application.Dashboard;
using AtelieBebe.Infrastructure.Notifications;
using AtelieBebe.Infrastructure.Outbox;
using AtelieBebe.Infrastructure.Payments;
using AtelieBebe.Infrastructure.Persistence;
using AtelieBebe.Infrastructure.Persistence.Queries;
using AtelieBebe.Infrastructure.Security;
using AtelieBebe.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AtelieBebe.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, bool isDevelopment = false)
    {
        services.AddSingleton<DomainEventsToOutboxInterceptor>();

        var connectionString = configuration.GetConnectionString("Default") ?? "Data Source=atelie-bebe.db";
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseSqlite(connectionString);
            options.AddInterceptors(sp.GetRequiredService<DomainEventsToOutboxInterceptor>());
        });

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<WhatsAppOptions>(configuration.GetSection(WhatsAppOptions.SectionName));
        services.Configure<MercadoPagoOptions>(configuration.GetSection(MercadoPagoOptions.SectionName));
        services.Configure<AppUrlOptions>(configuration.GetSection(AppUrlOptions.SectionName));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddHttpClient<INotificationSender, WhatsAppNotificationSender>(client =>
            client.BaseAddress = new Uri("https://graph.facebook.com/"));
        var mercadoPagoAccessToken = configuration[$"{MercadoPagoOptions.SectionName}:AccessToken"];
        if (isDevelopment && string.IsNullOrWhiteSpace(mercadoPagoAccessToken))
        {
            // No real credentials locally yet — swap in a gateway that simulates Checkout Pro
            // via our own SPA instead of skipping the payment step entirely (see FakePaymentGateway).
            services.AddScoped<IPaymentGateway, FakePaymentGateway>();
        }
        else
        {
            services.AddHttpClient<IPaymentGateway, MercadoPagoGateway>(client =>
                client.BaseAddress = new Uri("https://api.mercadopago.com/"));
        }

        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
