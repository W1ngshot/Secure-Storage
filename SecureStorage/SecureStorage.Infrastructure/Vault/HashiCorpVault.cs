using System.Text;
using System.Text.Json;
using SecureStorage.Core.Interfaces.Vault;
using SecureStorage.Domain.Exceptions;
using VaultSharp;
using VaultSharp.Core;
using VaultSharp.V1.SecretsEngines.Transit;

namespace SecureStorage.Infrastructure.Vault;

public class HashiCorpVault(IVaultClient vaultClient) : IKeyVault
{
    private static string GetKeyName(string userId) => $"user-{userId}";

    public async Task CreateUserKeyAsync(string userId, byte[] key)
    {
        var keyName = GetKeyName(userId);

        var data = new Dictionary<string, object>
        {
            { "key", Convert.ToBase64String(key) }
        };

        await vaultClient.V1.Secrets.KeyValue.V2.WriteSecretAsync(
            path: keyName,
            data: data,
            mountPoint: "secret"
        );
    }

    public async Task<byte[]> GetUserKeyAsync(string userId)
    {
        var keyName = GetKeyName(userId);

        try
        {
            var result = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(
                path: keyName,
                mountPoint: "secret"
            );

            if (result.Data.Data.TryGetValue("key", out var value) && value is JsonElement
                {
                    ValueKind: JsonValueKind.String
                } json)
            {
                return Convert.FromBase64String(json.GetString()!);
            }

            throw new Exception("Key not found or invalid format.");
        }
        catch (VaultApiException e) when (e.StatusCode == 404)
        {
            throw new NotFoundException("user");
        }
    }

    public async Task<string> EncryptAsync(string userId, string plainText)
    {
        var keyName = GetKeyName(userId);

        var options = new EncryptRequestOptions
        {
            Base64EncodedPlainText = Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText))
        };
        var result = await vaultClient.V1.Secrets.Transit.EncryptAsync(keyName, options, mountPoint: "transit");
        return result.Data.CipherText;
    }

    public async Task<string> DecryptAsync(string userId, string cipherText)
    {
        var keyName = GetKeyName(userId);

        var options = new DecryptRequestOptions
        {
            CipherText = cipherText,
        };

        var result = await vaultClient.V1.Secrets.Transit.DecryptAsync(keyName, options, mountPoint: "transit");
        return Encoding.UTF8.GetString(Convert.FromBase64String(result.Data.Base64EncodedPlainText));
    }

    public async Task<Dictionary<string, string>> EncryptLabeledBatchAsync(string userId,
        Dictionary<string, string> labeledFields)
    {
        var keyName = GetKeyName(userId);

        var items = labeledFields.Select(kvp => new EncryptionItem
        {
            Base64EncodedPlainText = Convert.ToBase64String(Encoding.UTF8.GetBytes(kvp.Value))
        }).ToList();

        var options = new EncryptRequestOptions
        {
            BatchedEncryptionItems = items
        };

        var result = await vaultClient.V1.Secrets.Transit.EncryptAsync(keyName, options, mountPoint: "transit");

        var cipherTexts = result.Data.BatchedResults.Select(r => r.CipherText).ToList();

        var labeledCipher = labeledFields.Keys
            .Zip(cipherTexts, (label, cipher) => new { label, cipher })
            .ToDictionary(x => x.label, x => x.cipher);

        return labeledCipher;
    }

    public async Task<Dictionary<string, string>> DecryptLabeledBatchAsync(string userId,
        Dictionary<string, string> labeledCipherTexts)
    {
        var keyName = GetKeyName(userId);

        var items = labeledCipherTexts.Select(kvp => new DecryptionItem
        {
            CipherText = kvp.Value
        }).ToList();

        var options = new DecryptRequestOptions
        {
            BatchedDecryptionItems = items
        };

        var result = await vaultClient.V1.Secrets.Transit.DecryptAsync(keyName, options, mountPoint: "transit");

        var plainTexts = result.Data.BatchedResults
            .Select(r => Encoding.UTF8.GetString(Convert.FromBase64String(r.Base64EncodedPlainText)))
            .ToList();

        var labeledPlain = labeledCipherTexts.Keys
            .Zip(plainTexts, (label, plain) => new { label, plain })
            .ToDictionary(x => x.label, x => x.plain);

        return labeledPlain;
    }
}