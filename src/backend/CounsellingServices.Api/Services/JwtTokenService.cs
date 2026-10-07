using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CounsellingServices.Api.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace CounsellingServices.Api.Services;

public sealed class JwtTokenService(IConfiguration configuration) : ITokenService
{
    public (string token, DateTimeOffset expiresAt) Generate(User user)
    {
        var key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured.");
        var issuer = configuration["Jwt:Issuer"] ?? "CounsellingServices";
        var audience = configuration["Jwt:Audience"] ?? "CounsellingServices.Client";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(8);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience, claims, expires: expiresAt.UtcDateTime, signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
