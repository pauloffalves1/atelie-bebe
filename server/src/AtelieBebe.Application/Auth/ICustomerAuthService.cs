namespace AtelieBebe.Application.Auth;

public interface ICustomerAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterCustomerRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<CustomerProfileDto> GetProfileAsync(Guid customerId, CancellationToken ct = default);
}
