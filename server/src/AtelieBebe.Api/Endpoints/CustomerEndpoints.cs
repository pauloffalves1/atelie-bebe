using AtelieBebe.Application.Customers;

namespace AtelieBebe.Api.Endpoints;

public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this WebApplication app)
    {
        var adminGroup = app.MapGroup("/api/admin/customers").WithTags("Clientes (admin)").RequireAuthorization("AdminOnly");

        adminGroup.MapGet("/", async (ICustomerAdminService service, CancellationToken ct) =>
            Results.Ok(await service.ListAsync(ct)));

        adminGroup.MapGet("/{id:guid}", async (Guid id, ICustomerAdminService service, CancellationToken ct) =>
            Results.Ok(await service.GetByIdAsync(id, ct)));

        adminGroup.MapPut("/{id:guid}", async (Guid id, UpdateCustomerRequest request, ICustomerAdminService service, CancellationToken ct) =>
            Results.Ok(await service.UpdateAsync(id, request, ct)));
    }
}
