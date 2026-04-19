namespace Identity.Application.DTOs
{
    public class TokenResult
    {
        public string AccessToken { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
    }
}