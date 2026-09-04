using AtelieBebe.Application.Contact;

namespace AtelieBebe.Api.Endpoints;

public static class ContactEndpoints
{
    public static void MapContactEndpoints(this WebApplication app)
    {
        app.MapPost("/api/contact", async (SubmitContactRequest request, IContactService service, CancellationToken ct) =>
        {
            await service.SubmitAsync(request, ct);
            return Results.NoContent();
        }).WithTags("Contato");

        app.MapGet("/api/admin/contact-messages", async (IContactService service, CancellationToken ct) =>
            Results.Ok(await service.ListAsync(ct)))
            .WithTags("Contato (admin)")
            .RequireAuthorization("AdminOnly");
    }
}
