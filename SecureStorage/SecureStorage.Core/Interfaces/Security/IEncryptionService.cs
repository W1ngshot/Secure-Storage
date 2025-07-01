namespace SecureStorage.Core.Interfaces.Security;

public interface IEncryptionService
{
    byte[] Encrypt(byte[] plainData, byte[] key);
    byte[] Decrypt(byte[] cipherData, byte[] key);
    string Encrypt<T>(T plainText, byte[] key);
    T Decrypt<T>(string encryptedText, byte[] key);
    string? TryDecrypt(byte[] cipherData, byte[] key);
    T? TryDecrypt<T>(string cipherData, byte[] key);
}