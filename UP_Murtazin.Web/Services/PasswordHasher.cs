using System.Security.Cryptography;

namespace UP_Murtazin.Web.Services;

public static class PasswordHasher
{
    private const int SaltSize = 32;
    private const int HashSize = 32;
    private const int Iterations = 10000;

    public static bool VerifyPassword(string password, string? hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            return false;
        }

        byte[] hashBytes = Convert.FromBase64String(hashedPassword);
        if (hashBytes.Length < SaltSize + HashSize)
        {
            return false;
        }

        byte[] salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        byte[] storedHash = new byte[HashSize];
        Array.Copy(hashBytes, SaltSize, storedHash, 0, HashSize);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        byte[] computedHash = pbkdf2.GetBytes(HashSize);
        return computedHash.SequenceEqual(storedHash);
    }
}
