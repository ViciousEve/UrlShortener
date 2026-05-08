namespace Analytics.Application.DTOs
{
    public sealed record UrlStatsResponse(
        Guid Id,
        string ShortCode,
        int TotalClicks,
        DateTime? LastClickedAtUtc
    );
}
