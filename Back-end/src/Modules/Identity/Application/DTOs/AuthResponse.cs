namespace Identity.Application.DTOs
{
    public class AuthResponse
    {
        /// <summary>The signed JWT token string.</summary>
        public string AccessToken { get; set; }

        /// <summary>When the token expires (UTC). Frontend can use this to schedule refresh.</summary>
        public DateTime ExpiresAtUtc { get; set; }
    }
}
