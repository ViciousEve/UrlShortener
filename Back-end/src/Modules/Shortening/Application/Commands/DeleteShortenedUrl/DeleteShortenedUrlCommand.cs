using MediatR;

namespace Shortening.Application.Commands.DeleteShortenedUrl
{
    public record DeleteShortenedUrlCommand(string ShortCode, Guid? UserId) : IRequest;
}