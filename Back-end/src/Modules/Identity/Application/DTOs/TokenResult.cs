namespace Identity.Application.DTOs
{
    public class TokenResult
    {
        public string Token { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
    }
}