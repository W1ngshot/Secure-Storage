using System.Security.Cryptography;
using SecureStorage.Domain.Security;

namespace SecureStorage.Infrastructure.Security;

public class Pbkdf2KdfService : IKdfService
{
    public byte[] DeriveKey(string password, byte[] salt, int keySize = 32)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(keySize);
    }
}