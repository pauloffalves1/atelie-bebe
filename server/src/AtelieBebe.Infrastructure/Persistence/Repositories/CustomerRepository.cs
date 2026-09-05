using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Repositories;
using AtelieBebe.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AtelieBebe.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _dbContext;

    public CustomerRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _dbContext.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = Email.Create(email);
        return _dbContext.Customers.FirstOrDefaultAsync(c => c.Email == normalized, ct);
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        var normalized = Email.Create(email);
        return _dbContext.Customers.AnyAsync(c => c.Email == normalized, ct);
    }

    public Task<bool> CpfExistsAsync(string cpf, CancellationToken ct = default)
    {
        var normalized = Cpf.Create(cpf);
        return _dbContext.Customers.AnyAsync(c => c.Cpf == normalized, ct);
    }

    public async Task<IReadOnlyList<Customer>> ListAsync(CancellationToken ct = default) =>
        await _dbContext.Customers.OrderBy(c => c.Name).ToListAsync(ct);

    public void Add(Customer customer) => _dbContext.Customers.Add(customer);
}
