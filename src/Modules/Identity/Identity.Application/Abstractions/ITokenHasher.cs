namespace Identity.Application.Abstractions;

/// <summary>
/// Сервис для хеширования токенов для безопасного хранения
/// </summary>
public interface ITokenHasher
{
    /// <summary>
    /// Хешировать токен
    /// </summary>
    string HashToken(string token);

    /// <summary>
    /// Проверить токен против хеша
    /// </summary>
    bool VerifyToken(string token, string tokenHash);
}

