using AtelieBebe.Application.Abstractions;

namespace AtelieBebe.Application.Customers;

public sealed class CustomerAdminService : ICustomerAdminService
{
    private readonly IUnitOfWork _unitOfWork;

    public CustomerAdminService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<CustomerSummaryDto>> ListAsync(CancellationToken ct = default)
    {
        var customers = await _unitOfWork.Customers.ListAsync(ct);
        return customers
            .Select(c => new CustomerSummaryDto(c.Id, c.Name, c.Email.Value, c.Phone, c.Cpf?.Value, c.CreatedAt))
            .ToList();
    }
}
