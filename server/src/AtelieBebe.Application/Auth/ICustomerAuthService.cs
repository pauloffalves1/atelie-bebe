namespace AtelieBebe.Application.Auth;

public interface ICustomerAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterCustomerRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<CustomerProfileDto> GetProfileAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>Always succeeds regardless of whether the e-mail is registered, so the response never leaks which accounts exist.</summary>
    Task RequestPasswordResetAsync(string email, CancellationToken ct = default);
    Task ResetPasswordAsync(string token, string newPassword, CancellationToken ct = default);
    Task DeleteAccountAsync(Guid customerId, string password, CancellationToken ct = default);
}
