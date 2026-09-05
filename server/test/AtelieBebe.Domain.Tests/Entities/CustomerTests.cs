using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Events;
using AtelieBebe.Domain.Exceptions;
using AtelieBebe.Domain.ValueObjects;

namespace AtelieBebe.Domain.Tests.Entities;

public class CustomerTests
{
    private static readonly Email CustomerEmail = Email.Create("cliente@ateliebebe.com.br");

    [Fact]
    public void Register_WithEmptyName_Throws()
    {
        Assert.Throws<DomainException>(() => Customer.Register(" ", CustomerEmail, "hash", "11999999999"));
    }

    [Fact]
    public void Register_WithEmptyPasswordHash_Throws()
    {
        Assert.Throws<DomainException>(() => Customer.Register("Maria Silva", CustomerEmail, "", "11999999999"));
    }

    [Fact]
    public void Register_WithEmptyPhone_Throws()
    {
        Assert.Throws<DomainException>(() => Customer.Register("Maria Silva", CustomerEmail, "hash", null));
    }

    [Fact]
    public void Register_Valid_RaisesCustomerRegisteredEvent()
    {
        var customer = Customer.Register("Maria Silva", CustomerEmail, "hashed-password", "11999999999");

        var raised = Assert.Single(customer.DomainEvents.OfType<CustomerRegisteredDomainEvent>());
        Assert.Equal(customer.Id, raised.CustomerId);
        Assert.Equal("Maria Silva", raised.Name);
        Assert.Equal("11999999999", raised.Phone);
    }

    [Fact]
    public void UpdatePassword_WithEmptyHash_Throws()
    {
        var customer = Customer.Register("Maria Silva", CustomerEmail, "hashed-password", "11999999999");

        Assert.Throws<DomainException>(() => customer.UpdatePassword(""));
    }

    [Fact]
    public void UpdatePassword_Valid_ReplacesHash()
    {
        var customer = Customer.Register("Maria Silva", CustomerEmail, "old-hash", "11999999999");

        customer.UpdatePassword("new-hash");

        Assert.Equal("new-hash", customer.PasswordHash);
    }
}
