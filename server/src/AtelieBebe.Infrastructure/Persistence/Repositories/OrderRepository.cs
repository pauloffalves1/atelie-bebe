using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Enums;
using AtelieBebe.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AtelieBebe.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _dbContext;

    public OrderRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _dbContext.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<(IReadOnlyList<Order> Items, int TotalItems)> ListAsync(OrderStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbContext.Orders.Include(o => o.Items).AsQueryable();

        if (status is not null)
            query = query.Where(o => o.Status == status);

        query = query.OrderByDescending(o => o.CreatedAt);

        var totalItems = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (items, totalItems);
    }

    public async Task<IReadOnlyList<Order>> ListByCustomerAsync(Guid customerId, CancellationToken ct = default) =>
        await _dbContext.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .ToListAsync(ct);

    public void Add(Order order) => _dbContext.Orders.Add(order);
}
