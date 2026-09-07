using System.Linq;
using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Events;
using AtelieBebe.Domain.Exceptions;
using AtelieBebe.Domain.ValueObjects;

namespace AtelieBebe.Domain.Tests.Entities;

public class CustomerTests
{
    private static readonly Email CustomerEmail = Email.Create("cliente@ateliebebe.com.br");
    private static readonly Cpf CustomerCpf = Cpf.Create("529.982.247-25");

    [Fact]
    public void Register_WithEmptyName_Throws()
    {
        Assert.Throws<DomainException>(() => Customer.Register(" ", CustomerEmail, CustomerCpf, "hash", "11999999999"));
    }

    [Fact]
    public void Register_WithNullCpf_Throws()
    {
        Assert.Throws<DomainException>(() => Customer.Register("Maria Silva", CustomerEmail, null!, "hash", "11999999999"));
    }

    [Fact]
    public void Register_WithEmptyPasswordHash_Throws()
    {
        Assert.Throws<DomainException>(() => Customer.Register("Maria Silva", CustomerEmail, CustomerCpf, "", "11999999999"));
    }

    [Fact]
    public void Register_WithEmptyPhone_Throws()
    {
        Assert.Throws<DomainException>(() => Customer.Register("Maria Silva", CustomerEmail, CustomerCpf, "hash", null));
    }

    [Fact]
    public void Register_Valid_RaisesCustomerRegisteredEvent()
    {
        var customer = Customer.Register("Maria Silva", CustomerEmail, CustomerCpf, "hashed-password", "11999999999");

        var raised = Assert.Single(customer.DomainEvents.OfType<CustomerRegisteredDomainEvent>());
        Assert.Equal(customer.Id, raised.CustomerId);
        Assert.Equal("Maria Silva", raised.Name);
        Assert.Equal("11999999999", raised.Phone);
    }

    [Fact]
    public void Register_WithAddress_SetsAddressFields()
    {
        var customer = Customer.Register(
            "Maria Silva", CustomerEmail, CustomerCpf, "hash", "11999999999",
            addressStreet: "Rua das Flores", addressNumber: "123", addressComplement: "Apto 4",
            addressNeighborhood: "Centro", addressCity: "São Paulo", addressState: "SP", addressZipCode: "01000-000");

        Assert.Equal("Rua das Flores", customer.AddressStreet);
        Assert.Equal("123", customer.AddressNumber);
        Assert.Equal("Apto 4", customer.AddressComplement);
        Assert.Equal("Centro", customer.AddressNeighborhood);
        Assert.Equal("São Paulo", customer.AddressCity);
        Assert.Equal("SP", customer.AddressState);
        Assert.Equal("01000-000", customer.AddressZipCode);
    }

    [Fact]
    public void Anonymize_ClearsAddressFields()
    {
        var customer = Customer.Register(
            "Maria Silva", CustomerEmail, CustomerCpf, "hash", "11999999999",
            addressStreet: "Rua das Flores", addressNumber: "123", addressCity: "São Paulo", addressState: "SP", addressZipCode: "01000-000");

        customer.Anonymize("unusable-hash");

        Assert.Null(customer.AddressStreet);
        Assert.Null(customer.AddressNumber);
        Assert.Null(customer.AddressCity);
        Assert.Null(customer.AddressState);
        Assert.Null(customer.AddressZipCode);
    }

    [Fact]
    public void UpdatePassword_WithEmptyHash_Throws()
    {
        var customer = Customer.Register("Maria Silva", CustomerEmail, CustomerCpf, "hashed-password", "11999999999");

        Assert.Throws<DomainException>(() => customer.UpdatePassword(""));
    }

    [Fact]
    public void UpdatePassword_Valid_ReplacesHash()
    {
        var customer = Customer.Register("Maria Silva", CustomerEmail, CustomerCpf, "old-hash", "11999999999");

        customer.UpdatePassword("new-hash");

        Assert.Equal("new-hash", customer.PasswordHash);
    }

    [Fact]
    public void UpdateDetails_Valid_ReplacesFields()
    {
        var customer = Customer.Register("Maria Silva", CustomerEmail, CustomerCpf, "hash", "11999999999");
        var newEmail = Email.Create("maria.nova@ateliebebe.com.br");
        var newCpf = Cpf.Create("111.444.777-35");

        customer.UpdateDetails("Maria Silva Souza", newEmail, newCpf, "11988887777");

        Assert.Equal("Maria Silva Souza", customer.Name);
        Assert.Equal(newEmail, customer.Email);
        Assert.Equal(newCpf, customer.Cpf);
        Assert.Equal("11988887777", customer.Phone);
    }

    [Fact]
    public void UpdateDetails_WithEmptyName_Throws()
    {
        var customer = Customer.Register("Maria Silva", CustomerEmail, CustomerCpf, "hash", "11999999999");

        Assert.Throws<DomainException>(() => customer.UpdateDetails(" ", CustomerEmail, CustomerCpf, "11999999999"));
    }

    [Fact]
    public void UpdateDetails_WithEmptyPhone_Throws()
    {
        var customer = Customer.Register("Maria Silva", CustomerEmail, CustomerCpf, "hash", "11999999999");

        Assert.Throws<DomainException>(() => customer.UpdateDetails("Maria Silva", CustomerEmail, CustomerCpf, null));
    }

    [Fact]
    public void RequestPasswordReset_Valid_RaisesEvent()
    {
        var customer = Customer.Register("Maria Silva", CustomerEmail, CustomerCpf, "hash", "11999999999");

        customer.RequestPasswordReset("https://layettebaby.com.br/redefinir-senha?token=abc");

        var domainEvent = Assert.Single(customer.DomainEvents.OfType<PasswordResetRequestedDomainEvent>());
        Assert.Equal(customer.Id, domainEvent.CustomerId);
        Assert.Equal("https://layettebaby.com.br/redefinir-senha?token=abc", domainEvent.ResetUrl);
    }

    [Fact]
    public void RequestPasswordReset_WithEmptyUrl_Throws()
    {
        var customer = Customer.Register("Maria Silva", CustomerEmail, CustomerCpf, "hash", "11999999999");

        Assert.Throws<DomainException>(() => customer.RequestPasswordReset(" "));
    }

    [Fact]
    public void Anonymize_ScrubsPersonalDataAndDisablesLogin()
    {
        var customer = Customer.Register("Maria Silva", CustomerEmail, CustomerCpf, "old-hash", "11999999999");

        customer.Anonymize("unusable-hash");

        Assert.True(customer.IsAnonymized);
        Assert.Equal("Cliente removido", customer.Name);
        Assert.Null(customer.Cpf);
        Assert.Null(customer.Phone);
        Assert.Equal("unusable-hash", customer.PasswordHash);
        Assert.NotEqual(CustomerEmail, customer.Email);
    }

    [Fact]
    public void Anonymize_CalledTwice_IsIdempotent()
    {
        var customer = Customer.Register("Maria Silva", CustomerEmail, CustomerCpf, "old-hash", "11999999999");

        customer.Anonymize("first-hash");
        var emailAfterFirst = customer.Email;
        customer.Anonymize("second-hash");

        Assert.Equal(emailAfterFirst, customer.Email);
        Assert.Equal("first-hash", customer.PasswordHash);
    }
}
