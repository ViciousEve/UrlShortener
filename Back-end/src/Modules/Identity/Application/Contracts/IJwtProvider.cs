using Identity.Domain;
using Identity.Application.DTOs;

namespace Identity.Application.Contracts
{
    /// <summary>
    /// Abstraction for JWT token generation.
    /// Keeps the Application layer independent of any specific JWT library.
    /// 
    /// IMPLEMENTATION NOTES:
    /// 
    /// 1. The implementation (JwtProvider.cs in Infrastructure/Services) will use
    ///    System.IdentityModel.Tokens.Jwt or Microsoft.IdentityModel.JsonWebTokens
    ///    to create signed JWT tokens.
    ///    
    /// 2. The generated token should include these claims:
    ///    - sub (Subject): user.Id
    ///    - email: user.Email.Value
    ///    - name: user.Username
    ///    - role: user.Role.ToString() — used for role-based authorization
    ///    - jti (JWT ID): a unique identifier for the token
    ///    - iat (Issued At): when the token was created
    ///    - exp (Expiration): when the token expires
    ///    
    /// 3. The implementation should read configuration from IConfiguration:
    ///    - Jwt:SecretKey — the signing key (HMAC-SHA256 or RSA)
    ///    - Jwt:Issuer — who issued the token
    ///    - Jwt:Audience — who the token is for
    ///    - Jwt:ExpirationInMinutes — token lifetime
    /// </summary>
    public interface IJwtProvider
    {
        TokenResult GenerateToken(User user);
    }
}
