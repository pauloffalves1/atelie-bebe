using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Repositories;
using AtelieBebe.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AtelieBebe.Infrastructure.Persistence.Repositories;

public sealed class AdminRepository : IAdminRepository
{
    private readonly AppDbContext _dbContext;

    public AdminRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<Admin?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = Email.Create(email);
        return _dbContext.Admins.FirstOrDefaultAsync(a => a.Email == normalized, ct);
    }

    public void Add(Admin admin) => _dbContext.Admins.Add(admin);
}
