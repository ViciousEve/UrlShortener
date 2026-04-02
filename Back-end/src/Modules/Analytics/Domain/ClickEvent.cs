using App.Abstractions;

namespace Analytics.Domain
{
    public class ClickEvent : Entity
    {
        public Guid Id { get; private set; }
        public Guid ShortenedUrlStatsId { get; private set; }
        public DateTime ClickedAtUtc { get; private set; }
        public string? IpAddress { get; private set; }
        public string? UserAgent { get; private set; }

        // Navigation property
        public ShortenedUrlStats ShortenedUrlStats { get; private set; }

        private ClickEvent() { } // Private parameterless constructor for EF Core

        public ClickEvent(Guid shortenedUrlStatsId, DateTime clickedAtUtc, string? ipAddress, string? userAgent)
        {
            Id = Guid.NewGuid();
            ShortenedUrlStatsId = shortenedUrlStatsId;
            ClickedAtUtc = clickedAtUtc;
            IpAddress = ipAddress;
            UserAgent = userAgent;
        }
    }
}
