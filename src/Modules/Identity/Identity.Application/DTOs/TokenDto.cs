namespace Identity.Application.DTOs;

/// <summary>
/// DTO для токена аутентификации
/// </summary>
public sealed class TokenDto
{
    /// <summary>
    /// Access токен (JWT)
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh токен
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Тип токена
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Время истечения access токена (в секундах)
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Дата истечения refresh токена
    /// </summary>
    public DateTime RefreshTokenExpiresAt { get; set; }
}

