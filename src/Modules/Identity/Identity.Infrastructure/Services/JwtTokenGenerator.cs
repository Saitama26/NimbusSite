using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Identity.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure.Services;

/// <summary>
/// Реализация генерации JWT токенов
/// </summary>
internal sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpirationMinutes;
    private readonly int _refreshTokenExpirationDays;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
        _secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
            ?? configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured.");
        
        _issuer = configuration["Jwt:Issuer"] ?? "NimbusSite";
        _audience = configuration["Jwt:Audience"] ?? "NimbusSite-Users";
        _accessTokenExpirationMinutes = int.Parse(
            configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");
        _refreshTokenExpirationDays = int.Parse(
            configuration["Jwt:RefreshTokenExpirationDays"] ?? "30");
    }

    public string GenerateAccessToken(Guid userId, Guid tenantId, IEnumerable<string>? roles = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("UserId", userId.ToString()),
            new Claim("TenantId", tenantId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, 
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), 
                ClaimValueTypes.Integer64)
        };

        if (roles != null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: GetAccessTokenExpiration(),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public DateTime GetAccessTokenExpiration()
    {
        return DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes);
    }

    public DateTime GetRefreshTokenExpiration()
    {
        return DateTime.UtcNow.AddDays(_refreshTokenExpirationDays);
    }
}

