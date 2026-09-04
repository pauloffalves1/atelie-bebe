using AtelieBebe.Application.Abstractions;
using AtelieBebe.Application.Dashboard;
using AtelieBebe.Infrastructure.Notifications;
using AtelieBebe.Infrastructure.Outbox;
using AtelieBebe.Infrastructure.Persistence;
using AtelieBebe.Infrastructure.Persistence.Queries;
using AtelieBebe.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AtelieBebe.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<DomainEventsToOutboxInterceptor>();

        var connectionString = configuration.GetConnectionString("Default") ?? "Data Source=atelie-bebe.db";
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseSqlite(connectionString);
            options.AddInterceptors(sp.GetRequiredService<DomainEventsToOutboxInterceptor>());
        });

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<INotificationSender, LoggingNotificationSender>();

        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
