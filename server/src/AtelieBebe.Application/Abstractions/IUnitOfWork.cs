using AtelieBebe.Domain.Repositories;

namespace AtelieBebe.Application.Abstractions;

/// <summary>
/// Coordinates the aggregate repositories that participate in a single business transaction
/// and commits them atomically. All repository writes only take effect after SaveChangesAsync.
/// </summary>
public interface IUnitOfWork
{
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    ICustomerRepository Customers { get; }
    IAdminRepository Admins { get; }
    IContactMessageRepository ContactMessages { get; }
    ISiteImageRepository SiteImages { get; }
    IGalleryImageRepository GalleryImages { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
