using AtelieBebe.Domain.Common;
using AtelieBebe.Domain.Events;
using AtelieBebe.Domain.Exceptions;
using AtelieBebe.Domain.ValueObjects;

namespace AtelieBebe.Domain.Entities;

public sealed class Customer : Entity, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string? Phone { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Customer() { } // EF Core

    private Customer(Guid id, string name, Email email, string passwordHash, string? phone)
        : base(id)
    {
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        Phone = phone;
        CreatedAt = DateTime.UtcNow;
    }

    public static Customer Register(string name, Email email, string passwordHash, string? phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("A senha é obrigatória.");

        var customer = new Customer(Guid.NewGuid(), name.Trim(), email, passwordHash, phone);
        customer.AddDomainEvent(new CustomerRegisteredDomainEvent(customer.Id, customer.Name, customer.Email.Value));
        return customer;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("A senha é obrigatória.");

        PasswordHash = newPasswordHash;
    }
}
