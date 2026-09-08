using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AtelieBebe.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _dbContext;

    public AuditLogRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public void Add(AuditLog log) => _dbContext.AuditLogs.Add(log);

    public async Task<(IReadOnlyList<AuditLog> Items, int TotalItems)> ListAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbContext.AuditLogs.OrderByDescending(a => a.CreatedAt);

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        var totalItems = await _dbContext.AuditLogs.CountAsync(ct);

        return (items, totalItems);
    }
}
