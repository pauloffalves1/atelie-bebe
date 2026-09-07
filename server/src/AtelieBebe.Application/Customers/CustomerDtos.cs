namespace AtelieBebe.Application.Customers;

public sealed record CustomerSummaryDto(Guid Id, string Name, string Email, string? Phone, string? Cpf, DateTime CreatedAt);

public sealed record UpdateCustomerRequest(string Name, string Email, string Cpf, string? Phone);
