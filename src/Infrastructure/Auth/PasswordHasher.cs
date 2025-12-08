using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Auth;

namespace Infrastructure.Auth;

public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));

        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            passwordBytes,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        var hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
            return false;

        try
        {
            var hashBytes = Convert.FromBase64String(passwordHash);

            if (hashBytes.Length != SaltSize + HashSize)
                return false;

            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                passwordBytes,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            for (var i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}