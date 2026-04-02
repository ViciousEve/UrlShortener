namespace Analytics.Application.DTOs
{
    public sealed record ClickEventResponse(
        Guid Id,
        Guid ShortenedUrlId,
        string ShortCode,
        string OriginalUrl,
        Guid? UserId,
        DateTime ClickedAtUtc
    );
}
