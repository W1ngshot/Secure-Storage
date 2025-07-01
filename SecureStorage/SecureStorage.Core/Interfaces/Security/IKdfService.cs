namespace SecureStorage.Core.Interfaces.Security;

public interface IKdfService
{
    byte[] DeriveKey(string password, byte[] salt, int keySize = 32);

    byte[] DeriveKey(string password, string userId, int keySize = 32);
    byte[] DeriveUserKey(byte[] key, string userId);

    byte[] DeriveCompositeKey(byte[] key, byte[] secret, int keySize = 32);

    byte[] DeriveCompositeKey(byte[] key, byte[] secret, string password, int keySize = 32);
}