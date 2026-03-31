using App.Abstractions;
using MediatR;

namespace Shortening.Application.IntegrationEvents
{
    public sealed record UrlClickedIntegrationEvent : IIntegrationEvent
    {
        public Guid Id { get; }
        public DateTime OccurredOnUtc { get; }
        public Guid ShortenedUrlId { get; }
        public string ShortCode { get; }
        public string OriginalUrl { get; }
        public Guid? UserId { get; }

        public UrlClickedIntegrationEvent(Guid shortenedUrlId, string shortCode, string originalUrl, Guid? userId)
        {
            Id = Guid.NewGuid();
            OccurredOnUtc = DateTime.UtcNow;
            ShortenedUrlId = shortenedUrlId;
            ShortCode = shortCode;
            OriginalUrl = originalUrl;
            UserId = userId; 
        }
    }
}