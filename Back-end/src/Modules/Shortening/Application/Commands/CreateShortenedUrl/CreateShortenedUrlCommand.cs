using MediatR;
using Shortening.Application.DTOs;

namespace Shortening.Application.Commands.CreateShortenedUrl
{
    public record CreateShortenedUrlCommand(string OriginalUrl, int TtlInMinutes, Guid? UserId) :IRequest<ShortenedUrlResponse>;
}