namespace AtelieBebe.Application.Auth;

public interface ICustomerAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterCustomerRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
