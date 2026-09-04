namespace AtelieBebe.Application.Auth;

public sealed record RegisterCustomerRequest(string Name, string Email, string Password, string? Phone);
public sealed record LoginRequest(string Email, string Password);
public sealed record AdminLoginRequest(string Email, string Password);

public sealed record AuthResponse(string Token, Guid Id, string Name, string Email);
