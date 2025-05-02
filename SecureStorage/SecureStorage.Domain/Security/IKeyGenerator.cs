namespace SecureStorage.Domain.Security;

public interface IKeyGenerator
{
    string GenerateKeyFromUserId(byte[] masterKey, string userId);

    string GenerateRandomKey(int keySize = 32);
    byte[] GenerateRandomByteKey(int keySize = 32);
}