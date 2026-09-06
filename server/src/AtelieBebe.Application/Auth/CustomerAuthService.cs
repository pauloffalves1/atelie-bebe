using AtelieBebe.Application.Abstractions;
using AtelieBebe.Application.Exceptions;
using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.ValueObjects;

namespace AtelieBebe.Application.Auth;

public sealed class CustomerAuthService : ICustomerAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public CustomerAuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterCustomerRequest request, CancellationToken ct = default)
    {
        if (await _unitOfWork.Customers.EmailExistsAsync(request.Email, ct))
            throw new ConflictException("Já existe uma conta com este e-mail.");

        var cpf = Cpf.Create(request.Cpf);
        if (await _unitOfWork.Customers.CpfExistsAsync(cpf.Value, ct))
            throw new ConflictException("Já existe uma conta com este CPF.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            throw new ConflictException("A senha deve ter pelo menos 6 caracteres.");

        var customer = Customer.Register(
            request.Name,
            Email.Create(request.Email),
            cpf,
            _passwordHasher.Hash(request.Password),
            request.Phone);

        _unitOfWork.Customers.Add(customer);
        await _unitOfWork.SaveChangesAsync(ct);

        var token = _jwtTokenGenerator.GenerateCustomerToken(customer);
        return new AuthResponse(token, customer.Id, customer.Name, customer.Email.Value);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var customer = await _unitOfWork.Customers.GetByEmailAsync(request.Email, ct);
        if (customer is null || !_passwordHasher.Verify(request.Password, customer.PasswordHash))
            throw new UnauthorizedAppException("E-mail ou senha inválidos.");

        var token = _jwtTokenGenerator.GenerateCustomerToken(customer);
        return new AuthResponse(token, customer.Id, customer.Name, customer.Email.Value);
    }

    public async Task<CustomerProfileDto> GetProfileAsync(Guid customerId, CancellationToken ct = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId, ct)
            ?? throw new NotFoundException("Cliente", customerId);

        return new CustomerProfileDto(customer.Id, customer.Name, customer.Email.Value, customer.Phone, customer.Cpf?.Value);
    }
}
