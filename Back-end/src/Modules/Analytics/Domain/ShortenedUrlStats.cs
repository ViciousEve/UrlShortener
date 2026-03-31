using App.Abstractions;

namespace Analytics.Domain
{
    public class ShortenedUrlStats : Entity
    {
        public Guid Id { get; private set; }
        public string ShortCode { get; private set; }
        public string OriginalUrl { get; private set; }
        public Guid? UserId { get; private set; }

        private ShortenedUrlStats() { } // Private parameterless constructor for EF Core

        public ShortenedUrlStats(Guid id, string shortCode, string originalUrl, Guid? userId)
        {
            if (string.IsNullOrEmpty(shortCode))
                throw new ArgumentException("Short code cannot be empty", nameof(shortCode));
            if (string.IsNullOrEmpty(originalUrl))
                throw new ArgumentException("Original URL cannot be empty", nameof(originalUrl));

            Id = id; // This will map to the original ShortenedUrlId
            ShortCode = shortCode;
            OriginalUrl = originalUrl;
            UserId = userId;
        }
    }
}
