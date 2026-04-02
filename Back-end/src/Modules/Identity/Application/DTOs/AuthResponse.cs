namespace Identity.Application.DTOs
{
    /// <summary>
    /// Response DTO returned after successful login or registration.
    /// Same pattern as ShortenedUrlResponse in the Shortening module.
    /// 
    /// IMPLEMENTATION NOTES:
    /// 
    /// 1. Token is the serialized JWT string (e.g., "eyJhbGciOi...").
    ///    The frontend stores this (typically in memory or httpOnly cookie)
    ///    and sends it in the Authorization header: "Bearer {token}".
    ///    
    /// 2. ExpiresAtUtc tells the frontend when to refresh/re-login.
    ///    This should match the token's "exp" claim.
    ///    
    /// 3. You might add more fields later:
    ///    - RefreshToken for silent re-authentication
    ///    - UserId, Email, Username for immediate display without decoding JWT
    /// </summary>
    public class AuthResponse
    {
        /// <summary>The signed JWT token string.</summary>
        public string Token { get; set; }

        /// <summary>When the token expires (UTC). Frontend can use this to schedule refresh.</summary>
        public DateTime ExpiresAtUtc { get; set; }
    }
}
