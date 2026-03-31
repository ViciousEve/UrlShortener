using Shortening.Application.Commands;
using Shortening.Application.Commands.CreateShortenedUrl;
using Shortening.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Shortening.Presentation
{
    public static class ShorteningEndpoint
    {

        public static void MapShorteningModule(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/shortening").WithTags("Shortening");

            group.MapPost("/shorten", async (CreateShortenUrlRequest request, IMediator mediator) =>
            {
                CreateShortenedUrlCommand command = new CreateShortenedUrlCommand(request.OriginalUrl, request.TtlMinutes, request.UserId);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            });

            group.MapGet("/Hi", () => "Hello Shortning module, it's working!");
        }
    }
}