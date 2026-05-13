using Shortening.Application.Commands.CreateShortenedUrl;
using Shortening.Application.Commands.DeleteShortenedUrl;
using Shortening.Application.DTOs;
using Shortening.Application.Queries.GetUserUrls;
using Shortening.Application.Queries.ResolveShortCode;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace Shortening.Presentation
{
    public static class ShorteningEndpoint
    {

        public static void MapShorteningModule(this IEndpointRouteBuilder app)
        {
            // ── Public redirect endpoint ────────────────────────────────
            app.MapGet("/s/{code}", async (string code, HttpContext httpContext, IMediator mediator) =>
            {
                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = httpContext.Request.Headers.UserAgent.ToString();

                var originalUrl = await mediator.Send(new ResolveShortCodeQuery(code, ipAddress, userAgent));
                return Results.Redirect(originalUrl, permanent: false);
            })
            .WithTags("Redirect")
            .WithName("ResolveShortCode")
            .WithDescription("Resolves a short code and redirects to the original URL");

            // ── API group (authenticated management endpoints) ──────────
            var group = app.MapGroup("/api/shortening").WithTags("Shortening");

            group.MapPost("/shorten", async (CreateShortenUrlRequest request, IMediator mediator) =>
            {
                CreateShortenedUrlCommand command = new CreateShortenedUrlCommand(request.OriginalUrl, request.TtlMinutes, request.UserId);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            group.MapGet("/urls", async (ClaimsPrincipal user, IMediator mediator) =>
            {
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await mediator.Send(new GetUserUrlsQuery(userId));
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetUserUrls")
            .WithDescription("Returns all shortened URLs for the authenticated user");

            group.MapDelete("/urls/{code}", async (string code, ClaimsPrincipal user, IMediator mediator) =>
            {
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await mediator.Send(new DeleteShortenedUrlCommand(code, userId));
                return Results.NoContent();
            })
            .RequireAuthorization()
            .WithName("DeleteShortenedUrl")
            .WithDescription("Deletes a shortened URL owned by the authenticated user");
        }
    }
}