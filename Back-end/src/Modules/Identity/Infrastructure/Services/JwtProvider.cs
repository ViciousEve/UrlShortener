using Identity.Application.Contracts;
using Identity.Domain;
using Identity.Application.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;


namespace Identity.Infrastructure.Services
{
    public class JwtProvider : IJwtProvider
    {
        private readonly IConfiguration _configuration;
        public JwtProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public TokenResult GenerateToken(User user)
        {
            var secretKey = _configuration["JwtSettings:SecretKey"];
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];
            var expirationMinutes = _configuration["JwtSettings:ExpirationMinutes"];

            if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience) || string.IsNullOrEmpty(expirationMinutes))
            {
                throw new InvalidOperationException("JWT settings are not configured");
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
                new Claim(JwtRegisteredClaimNames.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(expirationMinutes)),
                signingCredentials: credentials);

            return new TokenResult
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(int.Parse(expirationMinutes))
            };
        }
    }
}
