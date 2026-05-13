using Identity.Application.Commands.RegisterUser;
using Identity.Application.Queries.LoginUser;
using Identity.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Identity.Presentation
{
    public static class IdentityEndpoints
    {
        public static void MapIdentityModule(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/identity").WithTags("Identity");

            // POST /api/identity/register
            group.MapPost("/register", async (RegisterRequest request, IMediator mediator) =>
            {
                var command = new RegisterUserCommand(request.Email, request.Username, request.Password);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            })
            .WithName("RegisterUser")
            .AllowAnonymous();

            // POST /api/identity/login
            group.MapPost("/login", async (LoginRequest request, IMediator mediator) =>
            {
                var query = new LoginUserQuery(request.Email, request.Password);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("LoginUser")
            .AllowAnonymous();

            // Future endpoints:
            // GET  /api/identity/me             — Get current user profile (requires auth)
            // POST /api/identity/change-password — Change password (requires auth)
        }
    }
}
