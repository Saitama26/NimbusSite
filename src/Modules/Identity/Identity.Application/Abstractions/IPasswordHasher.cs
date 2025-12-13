namespace Identity.Application.Abstractions;

/// <summary>
/// Сервис для хеширования и проверки паролей
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Хешировать пароль
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Проверить пароль против хеша
    /// </summary>
    bool VerifyPassword(string password, string passwordHash);
}

