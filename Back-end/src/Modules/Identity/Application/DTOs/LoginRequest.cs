namespace Identity.Application.DTOs
{
    /// <summary>
    /// Request DTO for the login endpoint.
    /// 
    /// IMPLEMENTATION NOTES:
    /// 
    /// 1. Same pattern as RegisterRequest — simple POCO, no logic.
    /// 2. No Username field — login is by email + password.
    /// </summary>
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
