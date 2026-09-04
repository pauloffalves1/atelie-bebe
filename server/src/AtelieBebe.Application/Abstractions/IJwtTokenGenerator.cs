using AtelieBebe.Domain.Entities;

namespace AtelieBebe.Application.Abstractions;

public interface IJwtTokenGenerator
{
    string GenerateCustomerToken(Customer customer);
    string GenerateAdminToken(Admin admin);
}
