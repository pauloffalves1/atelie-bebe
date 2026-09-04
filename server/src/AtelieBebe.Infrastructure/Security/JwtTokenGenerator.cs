using System.Security.Claims;
using System.Text;
using AtelieBebe.Application.Abstractions;
using AtelieBebe.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AtelieBebe.Infrastructure.Security;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    public const string AdminRole = "admin";
    public const string CustomerRole = "customer";

    private readonly JwtOptions _options;
    private readonly JsonWebTokenHandler _handler = new();

    public JwtTokenGenerator(IOptions<JwtOptions> options) => _options = options.Value;

    public string GenerateCustomerToken(Customer customer) =>
        GenerateToken(customer.Id, customer.Name, customer.Email.Value, CustomerRole);

    public string GenerateAdminToken(Admin admin) =>
        GenerateToken(admin.Id, admin.Name, admin.Email.Value, AdminRole);

    private string GenerateToken(Guid id, string name, string email, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Expires = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
            SigningCredentials = credentials,
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
            }),
        };

        return _handler.CreateToken(descriptor);
    }
}
