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
    public IGalleryImageRepository GalleryImages { get; }
    public IProductReviewRepository ProductReviews { get; }
    public IPasswordResetTokenRepository PasswordResetTokens { get; }
    public ICouponRepository Coupons { get; }
    public IEmailVerificationTokenRepository EmailVerificationTokens { get; }
    public IWishlistItemRepository WishlistItems { get; }
    public ICartSnapshotRepository CartSnapshots { get; }
    public IAuditLogRepository AuditLogs { get; }
    public INewsletterSubscriberRepository NewsletterSubscribers { get; }

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        Products = new ProductRepository(dbContext);
        Orders = new OrderRepository(dbContext);
        Customers = new CustomerRepository(dbContext);
        Admins = new AdminRepository(dbContext);
        ContactMessages = new ContactMessageRepository(dbContext);
        SiteImages = new SiteImageRepository(dbContext);
        GalleryImages = new GalleryImageRepository(dbContext);
        ProductReviews = new ProductReviewRepository(dbContext);
        PasswordResetTokens = new PasswordResetTokenRepository(dbContext);
        Coupons = new CouponRepository(dbContext);
        EmailVerificationTokens = new EmailVerificationTokenRepository(dbContext);
        WishlistItems = new WishlistItemRepository(dbContext);
        CartSnapshots = new CartSnapshotRepository(dbContext);
        AuditLogs = new AuditLogRepository(dbContext);
        NewsletterSubscribers = new NewsletterSubscriberRepository(dbContext);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _dbContext.SaveChangesAsync(ct);
}
