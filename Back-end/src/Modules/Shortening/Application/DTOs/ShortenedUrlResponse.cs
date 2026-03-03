namespace Shortening.Application.DTOs
{
    public class ShortenedUrlResponse
    {
        public string ShortCode { get; set; }
        public string OriginalUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Status { get; set; }
    }
}