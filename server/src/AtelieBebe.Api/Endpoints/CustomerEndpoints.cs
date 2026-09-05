using AtelieBebe.Application.Customers;

namespace AtelieBebe.Api.Endpoints;

public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this WebApplication app)
    {
        var adminGroup = app.MapGroup("/api/admin/customers").WithTags("Clientes (admin)").RequireAuthorization("AdminOnly");

        adminGroup.MapGet("/", async (ICustomerAdminService service, CancellationToken ct) =>
            Results.Ok(await service.ListAsync(ct)));
    }
}
