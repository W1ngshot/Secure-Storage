namespace SecureStorage.Domain.Security;

public interface IEncryptionService
{
    byte[] Encrypt(byte[] plainData, byte[] key);
    byte[] Decrypt(byte[] cipherData, byte[] key);
}