using Identity.Application.Contracts;
using Identity.Domain;
using Identity.Application.DTOs;

namespace Identity.Infrastructure.Services
{
    /// <summary>
    /// JWT token generator implementation.
    /// Implements IJwtProvider from the Application layer.
    /// 
    /// IMPLEMENTATION NOTES:
    /// 
    /// 1. NuGet package needed: Microsoft.AspNetCore.Authentication.JwtBearer
    ///    (already included via FrameworkReference to Microsoft.AspNetCore.App)
    ///    You may also need: System.IdentityModel.Tokens.Jwt
    ///    
    /// 2. Read settings from IConfiguration (injected via DI):
    ///    - _configuration["Jwt:SecretKey"]   → signing key (min 32 chars for HMAC-SHA256)
    ///    - _configuration["Jwt:Issuer"]      → e.g., "UrlShortener"
    ///    - _configuration["Jwt:Audience"]    → e.g., "UrlShortener.Client"
    ///    - _configuration["Jwt:ExpirationInMinutes"] → e.g., "60"
    ///    
    /// 3. Steps to generate a token:
    ///    a. Create claims:
    ///       var claims = new[]
    ///       {
    ///           new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
    ///           new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
    ///           new Claim(JwtRegisteredClaimNames.Name, user.Username),
    ///           new Claim(ClaimTypes.Role, user.Role.ToString()),
    ///           new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    ///       };
    ///       
    ///    b. Create signing credentials:
    ///       var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    ///       var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    ///       
    ///    c. Create the token:
    ///       var token = new JwtSecurityToken(
    ///           issuer: issuer,
    ///           audience: audience,
    ///           claims: claims,
    ///           expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
    ///           signingCredentials: credentials);
    ///           
    ///    d. Serialize and return:
    ///       return new JwtSecurityTokenHandler().WriteToken(token);
    ///       
    /// 4. Add the Jwt section to appsettings.json:
    ///    "Jwt": {
    ///      "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharsLong!",
    ///      "Issuer": "UrlShortener",
    ///      "Audience": "UrlShortener.Client",
    ///      "ExpirationInMinutes": 60
    ///    }
    /// </summary>
    public class JwtProvider : IJwtProvider
    {
        // TODO: Inject IConfiguration via constructor
        // private readonly IConfiguration _configuration;
        // public JwtProvider(IConfiguration configuration)
        // {
        //     _configuration = configuration;
        // }

        /// <summary>
        /// IMPLEMENT: Generate a JWT token for the given user.
        /// Follow the steps in the class-level comments above.
        /// </summary>
        public TokenResult GenerateToken(User user)
        {
            // TODO: Implement — follow the steps above
            throw new NotImplementedException();
        }
    }
}
