using System.Security.Cryptography;
using System.Text;
using SecureStorage.Core.Interfaces.Security;

namespace SecureStorage.Infrastructure.Security;

public class Pbkdf2KdfService : IKdfService
{
    public byte[] DeriveKey(string password, byte[] salt, int keySize = 32)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(keySize);
    }

    public byte[] DeriveKey(string password, string userId, int keySize = 32)
    {
        using var pbkdf2 =
            new Rfc2898DeriveBytes(password, DeriveSaltFromUserId(userId), 100_000, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(keySize);
    }

    private static byte[] DeriveSaltFromUserId(string userId)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes("secure-storage-salt" + userId));
    }

    public byte[] DeriveUserKey(byte[] key, string userId)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(userId));
    }

    public byte[] DeriveCompositeKey(byte[] key, byte[] secret, int keySize = 32)
    {
        using var hmac = new HMACSHA256(key);
        var salt = hmac.ComputeHash(secret);

        using var pbkdf2 = new Rfc2898DeriveBytes(secret, salt, 100_000, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(keySize);
    }

    public byte[] DeriveCompositeKey(byte[] key, byte[] secret, string password, int keySize = 32)
    {
        using var hmac = new HMACSHA256(key);
        var salt = hmac.ComputeHash(secret);

        var pass = password + Convert.ToBase64String(secret);

        using var pbkdf2 = new Rfc2898DeriveBytes(pass, salt, 100_000, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(keySize);
    }
}