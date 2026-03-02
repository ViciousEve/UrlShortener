using App.Abstractions;

namespace Shortening.Domain.Events
{
    public sealed record UrlExpiredDomainEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredOnUtc { get; }
        public Guid ShortenedUrlId { get; }
        public string ShortCode { get; }

        public UrlExpiredDomainEvent(
            Guid shortenedUrlId,
            string shortCode)
        {
            Id = Guid.NewGuid();
            OccurredOnUtc = DateTime.UtcNow;
            ShortenedUrlId = shortenedUrlId;
            ShortCode = shortCode;
        }
    }
}
