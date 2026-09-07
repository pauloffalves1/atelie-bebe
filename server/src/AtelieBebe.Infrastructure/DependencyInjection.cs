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
using Microsoft.Extensions.Options;

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
        services.Configure<ResendOptions>(configuration.GetSection(ResendOptions.SectionName));
        services.Configure<PagBankOptions>(configuration.GetSection(PagBankOptions.SectionName));
        services.Configure<AppUrlOptions>(configuration.GetSection(AppUrlOptions.SectionName));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddHttpClient<INotificationSender, WhatsAppNotificationSender>(client =>
            client.BaseAddress = new Uri("https://graph.facebook.com/"));
        services.AddHttpClient<IEmailSender, ResendEmailSender>(client =>
            client.BaseAddress = new Uri("https://api.resend.com/"));
        var pagBankToken = configuration[$"{PagBankOptions.SectionName}:Token"];
        if (isDevelopment && string.IsNullOrWhiteSpace(pagBankToken))
        {
            // No real credentials locally yet — swap in a gateway that simulates the hosted
            // checkout via our own SPA instead of skipping the payment step entirely (see FakePaymentGateway).
            services.AddScoped<IPaymentGateway, FakePaymentGateway>();
        }
        else
        {
            services.AddHttpClient<IPaymentGateway, PagBankGateway>((sp, client) =>
            {
                var sandbox = sp.GetRequiredService<IOptions<PagBankOptions>>().Value.Sandbox;
                client.BaseAddress = new Uri(sandbox ? "https://sandbox.api.pagseguro.com/" : "https://api.pagseguro.com/");
            });
        }

        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
