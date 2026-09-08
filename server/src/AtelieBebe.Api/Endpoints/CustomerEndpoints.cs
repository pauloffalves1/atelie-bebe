using AtelieBebe.Api.Common;
using AtelieBebe.Application.Audit;
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

        adminGroup.MapPut("/{id:guid}", async (Guid id, UpdateCustomerRequest request, HttpContext http, ICustomerAdminService service, IAuditLogService auditLog, CancellationToken ct) =>
        {
            var updated = await service.UpdateAsync(id, request, ct);
            await auditLog.RecordAsync(http.User.GetUserId(), http.User.GetName(), "CustomerUpdated", $"Cliente '{updated.Name}' editado", ct);
            return Results.Ok(updated);
        });
    }
}
