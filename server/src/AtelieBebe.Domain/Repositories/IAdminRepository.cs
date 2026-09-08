using AtelieBebe.Domain.Entities;

namespace AtelieBebe.Domain.Repositories;

public interface IAdminRepository
{
    Task<Admin?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Admin?> GetByIdAsync(Guid id, CancellationToken ct = default);
    void Add(Admin admin);
}
