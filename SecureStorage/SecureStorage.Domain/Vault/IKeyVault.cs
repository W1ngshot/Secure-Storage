namespace SecureStorage.Domain.Vault;

public interface IKeyVault
{
    Task<byte[]> GetKeyForUserAsync(string userId);
}