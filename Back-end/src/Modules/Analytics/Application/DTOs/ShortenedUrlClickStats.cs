namespace Analytics.Application.DTOs
{
    public sealed record ShortenedUrlClickStats(
        Guid Id,
        string ShortCode,
        int TotalClicks
    );
}