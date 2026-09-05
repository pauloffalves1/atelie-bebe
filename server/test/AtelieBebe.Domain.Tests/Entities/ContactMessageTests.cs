using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Events;
using AtelieBebe.Domain.Exceptions;
using AtelieBebe.Domain.ValueObjects;

namespace AtelieBebe.Domain.Tests.Entities;

public class ContactMessageTests
{
    private static readonly Email SenderEmail = Email.Create("cliente@ateliebebe.com.br");

    [Fact]
    public void Create_WithEmptyName_Throws()
    {
        Assert.Throws<DomainException>(() => ContactMessage.Create(" ", SenderEmail, "11999999999", "Olá"));
    }

    [Fact]
    public void Create_WithEmptyPhone_Throws()
    {
        Assert.Throws<DomainException>(() => ContactMessage.Create("Maria Silva", SenderEmail, null!, "Olá"));
    }

    [Fact]
    public void Create_WithEmptyMessage_Throws()
    {
        Assert.Throws<DomainException>(() => ContactMessage.Create("Maria Silva", SenderEmail, "11999999999", " "));
    }

    [Fact]
    public void Create_Valid_RaisesContactMessageReceivedEvent()
    {
        var message = ContactMessage.Create("Maria Silva", SenderEmail, "11999999999", "Olá, gostaria de saber mais.");

        var raised = Assert.Single(message.DomainEvents.OfType<ContactMessageReceivedDomainEvent>());
        Assert.Equal(message.Id, raised.MessageId);
        Assert.Equal("Maria Silva", raised.Name);
        Assert.Equal("11999999999", raised.Phone);
    }
}
