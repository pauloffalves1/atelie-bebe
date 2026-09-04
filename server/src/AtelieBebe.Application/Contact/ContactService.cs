using AtelieBebe.Application.Abstractions;
using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.ValueObjects;

namespace AtelieBebe.Application.Contact;

public sealed class ContactService : IContactService
{
    private readonly IUnitOfWork _unitOfWork;

    public ContactService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task SubmitAsync(SubmitContactRequest request, CancellationToken ct = default)
    {
        var message = ContactMessage.Create(request.Name, Email.Create(request.Email), request.Message);
        _unitOfWork.ContactMessages.Add(message);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ContactMessageDto>> ListAsync(CancellationToken ct = default)
    {
        var messages = await _unitOfWork.ContactMessages.ListAsync(ct);
        return messages
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new ContactMessageDto(m.Id, m.Name, m.Email.Value, m.Message, m.CreatedAt))
            .ToList();
    }
}
