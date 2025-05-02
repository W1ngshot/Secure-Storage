namespace SecureStorage.Domain.Vault;

public interface IKeyVault
{
    Task CreateUserKeyAsync(string userId, byte[] key);
    Task<byte[]> GetUserKeyAsync(string userId);
    Task<string> EncryptAsync(string userId, string plainText);
    Task<string> DecryptAsync(string userId, string cipherText);
    Task<Dictionary<string, string>> EncryptLabeledBatchAsync(string userId, Dictionary<string, string> labeledFields);
    Task<Dictionary<string, string>> DecryptLabeledBatchAsync(string userId, Dictionary<string, string> labeledCipherTexts);
}