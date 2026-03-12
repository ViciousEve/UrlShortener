using System;
using App.Abstractions;
using Shortening.Domain.Events;

namespace Shortening.Domain
{
    public class ShortenedUrl : Entity
    {
        public Guid Id { get; private set; }
        public string OriginalUrl { get; private set; }
        public ShortCode ShortCode { get; private set; }
        public UrlStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public Guid? UserId { get; private set; } // Null for anonymous users

        private ShortenedUrl() { } //Private paramaterles constructor for EF code
        public ShortenedUrl(string originalUrl, ShortCode shortCode,
                            DateTime expiresAt, Guid? userId)
        {
            // Validate input
            if (string.IsNullOrEmpty(originalUrl))
                throw new ArgumentException("Original URL cannot be empty", nameof(originalUrl));
            if (shortCode == null)
                throw new ArgumentNullException(nameof(shortCode));
            if (expiresAt < DateTime.UtcNow)
                throw new ArgumentException("Expires at must be in the future", nameof(expiresAt));

            Id = Guid.NewGuid();
            OriginalUrl = originalUrl;
            ShortCode = shortCode;
            Status = UrlStatus.Active;
            CreatedAt = DateTime.UtcNow;
            ExpiresAt = expiresAt;
            UserId = userId;

            Raise(new UrlCreatedDomainEvent(Id, ShortCode.Value, OriginalUrl, UserId, ExpiresAt));
        }

        public void Disable()
        {
            if (Status == UrlStatus.Expired)
                throw new InvalidOperationException("Cannot disable an expired URL");
            Status = UrlStatus.Disabled;
        }

        public void Expire()
        {
            Status = UrlStatus.Expired;
            Raise(new UrlExpiredDomainEvent(Id, ShortCode.Value));
        }
    }
}
