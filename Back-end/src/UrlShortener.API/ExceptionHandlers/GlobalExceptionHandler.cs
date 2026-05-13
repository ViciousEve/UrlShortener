using App.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UrlShortener.API.ExceptionHandlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            NotFoundException notFound => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = notFound.Message
            },

            ExpiredUrlException expired => new ProblemDetails
            {
                Status = StatusCodes.Status410Gone,
                Title = "Gone",
                Detail = expired.Message
            },

            ForbiddenAccessException forbidden => new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = forbidden.Message
            },

            ConflictException conflict => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = conflict.Message
            },

            UnauthorizedException unauthorized => new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = unauthorized.Message
            },

            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server Error",
                Detail = "An unexpected error occurred. Please try again later."
            }
        };

        // Only log unexpected exceptions at Error level
        if (problemDetails.Status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "An unexpected error occurred.");
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception: {ExceptionType}", exception.GetType().Name);
        }

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
