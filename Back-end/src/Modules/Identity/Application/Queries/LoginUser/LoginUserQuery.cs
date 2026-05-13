using MediatR;
using Identity.Application.DTOs;

namespace Identity.Application.Queries.LoginUser
{
    /// <summary>
    /// MediatR query for user login.
    /// Modeled as a Query (not a Command) because login is a read operation:
    /// it verifies credentials and returns a token — no state mutation.
    /// 
    /// IMPLEMENTATION NOTES:
    /// 
    /// 1. Same record pattern as commands, but semantically a query.
    ///    Some teams put login under Commands since it has "side effects"
    ///    (token generation). Either approach is fine — the key is consistency.
    ///    
    /// 2. Contains raw password — the handler will verify it against the
    ///    stored hash via IPasswordHasher.Verify().
    ///    
    /// 3. Returns AuthResponse with the JWT token on success.
    ///    On failure (wrong email/password), the handler should throw
    ///    an exception that maps to 401 Unauthorized.
    /// </summary>
    public record LoginUserQuery(
        string Email,
        string Password
    ) : IRequest<AuthResponse>;
}
