using AtelieBebe.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AtelieBebe.Api.Health;

/// <summary>Confirms the API can actually reach the SQLite database, not just that the process is alive.</summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;

    public DatabaseHealthCheck(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            return await _dbContext.Database.CanConnectAsync(ct)
                ? HealthCheckResult.Healthy("Banco de dados acessível.")
                : HealthCheckResult.Unhealthy("Não foi possível conectar ao banco de dados.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Erro ao verificar o banco de dados.", ex);
        }
    }
}
