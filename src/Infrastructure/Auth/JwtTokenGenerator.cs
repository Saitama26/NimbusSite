using Application.Abstractions.Auth;
using Domain.Users;
using Domain.Users.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Auth;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpirationMinutes;
    private readonly int _refreshTokenExpirationDays;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
        _tokenHandler = new JwtSecurityTokenHandler();
        
        // Читаем из переменных окружения или из конфигурации
        _secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
            ?? _configuration["Jwt:SecretKey"] 
            ?? throw new InvalidOperationException("JWT_SECRET_KEY or Jwt:SecretKey is not configured");
        
        _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
            ?? _configuration["Jwt:Issuer"] 
            ?? "NimbusClone";
        
        _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
            ?? _configuration["Jwt:Audience"] 
            ?? "NimbusClone";
        
        var accessTokenExpiration = Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRATION_MINUTES") 
            ?? _configuration["Jwt:AccessTokenExpirationMinutes"] 
            ?? "60";
        _accessTokenExpirationMinutes = int.Parse(accessTokenExpiration);
        
        var refreshTokenExpiration = Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION_DAYS") 
            ?? _configuration["Jwt:RefreshTokenExpirationDays"] 
            ?? "30";
        _refreshTokenExpirationDays = int.Parse(refreshTokenExpiration);
    }

    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("TenantId", user.TenantId.ToString()),
            new Claim("Role", user.Role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = credentials
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return _tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public Guid? GetUserIdFromToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = false // Не проверяем срок действия для извлечения ID
            };

            var principal = _tokenHandler.ValidateToken(token, validationParameters, out _);
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public bool IsTokenValid(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            _tokenHandler.ValidateToken(token, validationParameters, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }
}