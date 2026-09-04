using AtelieBebe.Domain.Common;
using AtelieBebe.Domain.Events;
using AtelieBebe.Domain.Exceptions;
using AtelieBebe.Domain.ValueObjects;

namespace AtelieBebe.Domain.Entities;

public sealed class ContactMessage : Entity, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public string Message { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    private ContactMessage() { } // EF Core

    private ContactMessage(Guid id, string name, Email email, string message) : base(id)
    {
        Name = name;
        Email = email;
        Message = message;
        CreatedAt = DateTime.UtcNow;
    }

    public static ContactMessage Create(string name, Email email, string message)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(message))
            throw new DomainException("A mensagem é obrigatória.");

        var contactMessage = new ContactMessage(Guid.NewGuid(), name.Trim(), email, message.Trim());
        contactMessage.AddDomainEvent(new ContactMessageReceivedDomainEvent(contactMessage.Id, contactMessage.Name, contactMessage.Email.Value));
        return contactMessage;
    }
}
