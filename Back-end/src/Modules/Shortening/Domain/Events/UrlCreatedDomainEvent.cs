using App.Abstractions;
namespace Shortening.Domain.Events
{
    public sealed record UrlCreatedDomainEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredOnUtc { get; }
        public Guid ShortenedUrlId { get; }
        public string ShortCode { get; }
        public string OriginalUrl { get; }
        public Guid? UserId { get; }
        public DateTime ExpiresAtUtc { get; }

        public UrlCreatedDomainEvent(
            Guid shortenedUrlId,
            string shortCode,
            string originalUrl,
            Guid? userId,
            DateTime expiresAtUtc)
        {
            Id = Guid.NewGuid();
            OccurredOnUtc = DateTime.UtcNow;
            ShortenedUrlId = shortenedUrlId;
            ShortCode = shortCode;
            OriginalUrl = originalUrl;
            UserId = userId;
            ExpiresAtUtc = expiresAtUtc;
        }
    }
}