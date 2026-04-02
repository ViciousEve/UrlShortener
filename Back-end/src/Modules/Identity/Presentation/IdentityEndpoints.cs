using Identity.Application.Commands.RegisterUser;
using Identity.Application.Queries.LoginUser;
using Identity.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Identity.Presentation
{
    /// <summary>
    /// Minimal API endpoints for the Identity module.
    /// Same pattern as ShorteningEndpoints in the Shortening module.
    /// 
    /// IMPLEMENTATION NOTES:
    /// 
    /// 1. Endpoints are defined as extension methods on IEndpointRouteBuilder
    ///    and mapped in Program.cs via app.MapIdentityModule().
    ///    
    /// 2. The group prefix "/api/identity" keeps all Identity routes namespaced.
    ///    WithTags("Identity") groups them in Swagger/OpenAPI.
    ///    
    /// 3. Each endpoint:
    ///    a. Receives a request DTO from the HTTP body
    ///    b. Maps it to a MediatR command/query
    ///    c. Sends it through the pipeline (validation → handler)
    ///    d. Returns the result
    ///    
    /// 4. Later you can add more endpoints:
    ///    - GET /api/identity/me — get current user profile (requires auth)
    ///    - POST /api/identity/refresh — refresh the JWT token
    ///    - POST /api/identity/change-password
    /// </summary>
    public static class IdentityEndpoints
    {
        public static void MapIdentityModule(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/identity").WithTags("Identity");

            // TODO: Implement the register endpoint
            // group.MapPost("/register", async (RegisterRequest request, IMediator mediator) =>
            // {
            //     var command = new RegisterUserCommand(request.Email, request.Username, request.Password);
            //     var result = await mediator.Send(command);
            //     return Results.Ok(result);
            // });

            // TODO: Implement the login endpoint
            // group.MapPost("/login", async (LoginRequest request, IMediator mediator) =>
            // {
            //     var query = new LoginUserQuery(request.Email, request.Password);
            //     var result = await mediator.Send(query);
            //     return Results.Ok(result);
            // });

            // Health check endpoint (can be used immediately)
            group.MapGet("/hi", () => "Hello Identity module, it's working!");
        }
    }
}
