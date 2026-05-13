using MediatR;

namespace Shortening.Application.Commands.DisableShortenedUrl
{
    public record DisableShortenedUrlCommand(string ShortCode, Guid UserId) : IRequest;
}