using Analytics.Application.Queries.GetClicksByShortCode;
using Analytics.Application.Queries.GetUserClicksInPeriod;
using Analytics.Application.Queries.GetTopUrl;
using Analytics.Application.Queries.GetTopBrowser;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace Analytics.Presentation;

public static class AnalyticsEndpoints
{
    public static void MapAnalyticsModule(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/analytics").WithTags("Analytics");

        // GET /api/analytics/{code}/clicks — Click history for a specific short code
        group.MapGet("/{code}/clicks", async (string code, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetClicksByShortCodeQuery(code));
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetClicksByShortCode");

        // GET /api/analytics/clicks-in-period?from=...&to=... — Total clicks in date range
        group.MapGet("/clicks-in-period", async (DateTime from, DateTime to, ClaimsPrincipal user, IMediator mediator) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await mediator.Send(new GetUserClicksInPeriodQuery(userId, from, to));
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetUserClicksInPeriod");

        // GET /api/analytics/top-url — User's best performing URL
        group.MapGet("/top-url", async (ClaimsPrincipal user, IMediator mediator) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await mediator.Send(new GetTopUrlQuery(userId));
            return result is not null ? Results.Ok(result) : Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("GetTopUrl");

        // GET /api/analytics/top-browsers?topN=5 — Top browsers by click count
        group.MapGet("/top-browsers", async (ClaimsPrincipal user, IMediator mediator, int topN = 5) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await mediator.Send(new GetTopBrowserQuery(userId, topN));
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetTopBrowsers");
    }
}