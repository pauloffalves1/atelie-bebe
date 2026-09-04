using AtelieBebe.Application.Abstractions;
using AtelieBebe.Application.Exceptions;

namespace AtelieBebe.Application.Auth;

public sealed class AdminAuthService : IAdminAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AdminAuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> LoginAsync(AdminLoginRequest request, CancellationToken ct = default)
    {
        var admin = await _unitOfWork.Admins.GetByEmailAsync(request.Email, ct);
        if (admin is null || !_passwordHasher.Verify(request.Password, admin.PasswordHash))
            throw new UnauthorizedAppException("E-mail ou senha inválidos.");

        var token = _jwtTokenGenerator.GenerateAdminToken(admin);
        return new AuthResponse(token, admin.Id, admin.Name, admin.Email.Value);
    }
}
