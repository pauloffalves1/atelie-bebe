namespace AtelieBebe.Application.Customers;

public interface ICustomerAdminService
{
    Task<IReadOnlyList<CustomerSummaryDto>> ListAsync(CancellationToken ct = default);
}
