using AtelieBebe.Application.Abstractions;
using AtelieBebe.Domain.Repositories;
using AtelieBebe.Infrastructure.Persistence.Repositories;

namespace AtelieBebe.Infrastructure.Persistence;

/// <summary>
/// Wraps a single AppDbContext instance (scoped per request) so every repository shares the same
/// change tracker; SaveChangesAsync commits every aggregate change made through it as one transaction,
/// with the outbox interceptor appending domain events to that same commit.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public IProductRepository Products { get; }
    public IOrderRepository Orders { get; }
    public ICustomerRepository Customers { get; }
    public IAdminRepository Admins { get; }
    public IContactMessageRepository ContactMessages { get; }
    public ISiteImageRepository SiteImages { get; }

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        Products = new ProductRepository(dbContext);
        Orders = new OrderRepository(dbContext);
        Customers = new CustomerRepository(dbContext);
        Admins = new AdminRepository(dbContext);
        ContactMessages = new ContactMessageRepository(dbContext);
        SiteImages = new SiteImageRepository(dbContext);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _dbContext.SaveChangesAsync(ct);
}
