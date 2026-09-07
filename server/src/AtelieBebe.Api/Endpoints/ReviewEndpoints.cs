using AtelieBebe.Api.Common;
using AtelieBebe.Application.Reviews;

namespace AtelieBebe.Api.Endpoints;

public static class ReviewEndpoints
{
    public static void MapReviewEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products/{productId:guid}/reviews").WithTags("Avaliações");

        group.MapGet("/", async (Guid productId, IReviewService service, CancellationToken ct) =>
            Results.Ok(await service.ListByProductAsync(productId, ct)));

        group.MapGet("/eligibility", async (Guid productId, HttpContext http, IReviewService service, CancellationToken ct) =>
            Results.Ok(await service.GetEligibilityAsync(productId, http.User.GetUserId(), ct)))
            .RequireAuthorization("CustomerOnly");

        group.MapPost("/", async (Guid productId, CreateReviewRequest request, HttpContext http, IReviewService service, CancellationToken ct) =>
            Results.Ok(await service.CreateAsync(productId, http.User.GetUserId(), request, ct)))
            .RequireAuthorization("CustomerOnly");
    }
}
