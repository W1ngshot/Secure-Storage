using System.Security.Cryptography;
using System.Text;
using SecureStorage.Domain.Vault;

namespace SecureStorage.Infrastructure.Vault;

public class MockKeyVault : IKeyVault
{
    public Task<byte[]> GetKeyForUserAsync(string userId)
    {
        var hashed = SHA256.HashData(Encoding.UTF8.GetBytes($"vault:{userId}"));
        return Task.FromResult(hashed);
    }
}