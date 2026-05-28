using System.Security.Cryptography;
using System.Text;

namespace BLL.Helpers;

internal static class PasswordHasher
{
    private const int Iterations = 100_000;
    private const int HashSize = 32;   
    private const int SaltSize = 16;   
    private const char Separator = ':';

    internal static string Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return $"{Convert.ToBase64String(salt)}{Separator}{Convert.ToBase64String(hash)}";
    }

    internal static bool Verify(string password, string hashedPassword)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(hashedPassword);

        int separatorIndex = hashedPassword.IndexOf(Separator, StringComparison.Ordinal);
        if (separatorIndex < 0)
            return false;

        byte[] salt = Convert.FromBase64String(hashedPassword[..separatorIndex]);
        byte[] expectedHash = Convert.FromBase64String(hashedPassword[(separatorIndex + 1)..]);

        byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
