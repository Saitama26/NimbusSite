using BCrypt.Net;
using Identity.Application.Abstractions;

namespace Identity.Infrastructure.Services;

/// <summary>
/// Реализация хеширования паролей с использованием BCrypt
/// </summary>
internal sealed class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12; // BCrypt work factor (2^12 = 4096 iterations)

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        }

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }
}

