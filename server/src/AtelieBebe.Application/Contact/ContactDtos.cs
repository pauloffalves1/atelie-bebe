namespace AtelieBebe.Application.Contact;

public sealed record SubmitContactRequest(string Name, string Email, string Message);

public sealed record ContactMessageDto(Guid Id, string Name, string Email, string Message, DateTime CreatedAt);
