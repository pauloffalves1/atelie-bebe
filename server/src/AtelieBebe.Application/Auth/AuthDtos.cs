namespace AtelieBebe.Application.Auth;

public sealed record RegisterCustomerRequest(
    string Name, string Email, string Cpf, string Password, string? Phone,
    string? AddressStreet = null, string? AddressNumber = null, string? AddressComplement = null,
    string? AddressNeighborhood = null, string? AddressCity = null, string? AddressState = null, string? AddressZipCode = null);

public sealed record LoginRequest(string Email, string Password);
public sealed record AdminLoginRequest(string Email, string Password);

public sealed record AuthResponse(string Token, Guid Id, string Name, string Email);

public sealed record CustomerProfileDto(
    Guid Id, string Name, string Email, string? Phone, string? Cpf,
    string? AddressStreet, string? AddressNumber, string? AddressComplement,
    string? AddressNeighborhood, string? AddressCity, string? AddressState, string? AddressZipCode);

public sealed record ForgotPasswordRequest(string Email);
public sealed record ResetPasswordRequest(string Token, string NewPassword);
public sealed record DeleteAccountRequest(string Password);
