using System;
using System.Security.Cryptography;
using System.Text;

namespace UP_Murtazin.Services
{
    public class PasswordHasher
    {
        private const int SaltSize = 32;
        private const int HashSize = 32;
        private const int Iterations = 10000;

        /// <summary>
        /// Создает хеш пароля с солью
        /// </summary>
        public static string HashPassword(string password)
        {
            // Генерация соли
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Хеширование пароля с солью
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);

                // Формируем строку: соль + хеш
                byte[] hashBytes = new byte[SaltSize + HashSize];
                Array.Copy(salt, 0, hashBytes, 0, SaltSize);
                Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// Проверяет пароль на соответствие хешу
        /// </summary>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword))
                return false;

            byte[] hashBytes = Convert.FromBase64String(hashedPassword);

            // Извлекаем соль
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Извлекаем хеш
            byte[] storedHash = new byte[HashSize];
            Array.Copy(hashBytes, SaltSize, storedHash, 0, HashSize);

            // Хешируем введенный пароль с той же солью
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] computedHash = pbkdf2.GetBytes(HashSize);

                // Сравниваем хеши
                for (int i = 0; i < HashSize; i++)
                {
                    if (computedHash[i] != storedHash[i])
                        return false;
                }
            }

            return true;
        }
    }
}