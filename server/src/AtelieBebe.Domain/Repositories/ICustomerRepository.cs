using AtelieBebe.Domain.Entities;

namespace AtelieBebe.Domain.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<bool> CpfExistsAsync(string cpf, CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> ListAsync(CancellationToken ct = default);
    void Add(Customer customer);
}
