namespace Shortening.Application.DTOs
{
    public class CreateShortenUrlRequest
    {
        public string OriginalUrl { get; set; }
        public int TtlMinutes { get; set; }
        public Guid? UserId { get; set; }
    }
}