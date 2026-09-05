using AtelieBebe.Application.Dashboard;
using AtelieBebe.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AtelieBebe.Infrastructure.Persistence.Queries;

public sealed class DashboardService : IDashboardService
{
    private readonly AppDbContext _dbContext;

    public DashboardService(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task<DashboardDto> GetSummaryAsync(CancellationToken ct = default)
    {
        // Orders/Items are materialized as full aggregates (not a SQL-side projection) because
        // Money is mapped via a value converter: EF can hydrate it into an entity, but it cannot
        // translate a sub-member access like `i.UnitPrice.Amount` into SQL. The small size of a
        // boutique store's order history makes summing client-side with Order.Total perfectly fine.
        var orders = await _dbContext.Orders
            .Include(o => o.Items)
            .Where(o => o.Status != OrderStatus.Cancelado)
            .ToListAsync(ct);

        var totalProducts = await _dbContext.Products.CountAsync(ct);
        var totalCustomers = await _dbContext.Customers.CountAsync(ct);

        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var ordersByStatus = orders
            .GroupBy(o => o.Status)
            .Select(g => new OrdersByStatusDto(g.Key.ToString(), g.Count()))
            .OrderBy(s => s.Status)
            .ToList();

        var recentOrders = orders
            .OrderByDescending(o => o.CreatedAt)
            .Take(8)
            .Select(o => new RecentOrderSummaryDto(o.Id, o.CustomerName, o.Status.ToString(), o.Total.Amount, o.CreatedAt))
            .ToList();

        return new DashboardDto(
            TotalOrders: orders.Count,
            OpenOrders: orders.Count(o => o.Status is OrderStatus.Recebido or OrderStatus.EmProducao or OrderStatus.Pronto or OrderStatus.Enviado),
            RevenueTotal: orders.Sum(o => o.Total.Amount),
            RevenueThisMonth: orders.Where(o => o.CreatedAt >= startOfMonth).Sum(o => o.Total.Amount),
            TotalProducts: totalProducts,
            TotalCustomers: totalCustomers,
            OrdersByStatus: ordersByStatus,
            RecentOrders: recentOrders);
    }
}
