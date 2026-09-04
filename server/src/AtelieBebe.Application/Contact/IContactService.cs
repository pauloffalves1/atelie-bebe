namespace AtelieBebe.Application.Contact;

public interface IContactService
{
    Task SubmitAsync(SubmitContactRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ContactMessageDto>> ListAsync(CancellationToken ct = default);
}
