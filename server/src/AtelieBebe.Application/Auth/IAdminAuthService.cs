namespace AtelieBebe.Application.Auth;

public interface IAdminAuthService
{
    Task<AuthResponse> LoginAsync(AdminLoginRequest request, CancellationToken ct = default);
}
