namespace SecureStorage.Domain.Security;

public interface IKdfService
{
    byte[] DeriveKey(string password, byte[] salt, int keySize = 32);
}