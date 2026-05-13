namespace Identity.Application.DTOs
{
    /// <summary>
    /// Request DTO for the registration endpoint.
    /// Same pattern as CreateShortenUrlRequest in the Shortening module.
    /// 
    /// IMPLEMENTATION NOTES:
    /// 
    /// 1. This is a simple POCO — no validation logic here.
    ///    Validation happens in RegisterUserValidator via FluentValidation.
    ///    
    /// 2. The endpoint (IdentityEndpoints.cs) receives this from the HTTP body,
    ///    maps it to a RegisterUserCommand, and sends it via MediatR.
    ///    
    /// 3. Keeping DTOs separate from Commands/Queries decouples the API contract
    ///    from the internal CQRS model. You can change one without affecting the other.
    /// </summary>
    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
