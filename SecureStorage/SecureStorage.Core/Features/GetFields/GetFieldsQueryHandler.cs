using SecureStorage.Core.Extensions;
using SecureStorage.Domain.Entities;
using SecureStorage.Domain.Persistence;
using SecureStorage.Domain.Security;
using SecureStorage.Domain.Utility;
using SecureStorage.Domain.Vault;

namespace SecureStorage.Core.Features.GetFields;

public class GetFieldsQueryHandler(
    IKeyValueStorage storage,
    IEncryptionService encryption,
    IKdfService kdf,
    IKeyGenerator keyGenerator,
    IKeyVault vault,
    IDateTimeProvider dateTimeProvider)
{
    public async Task<GetFieldsQueryResponse> HandleAsync(GetFieldsQuery query)
    {
        var vaultKey = await vault.GetUserKeyAsync(query.UserId);
        var storageKey = keyGenerator.GenerateKeyFromUserId(vaultKey, query.UserId);
        var level1Key = kdf.DeriveUserKey(vaultKey, query.UserId);

        var response = new GetFieldsQueryResponse(query.Level1Fields, query.Level2Fields);

        using var transaction = storage.BeginTransaction();

        var level1 = transaction.GetAndDecryptOrThrow<Level1>(
            storageKey,
            encryption,
            level1Key);
        level1.LastAccessedAt = dateTimeProvider.UtcNow;

        var requiredLevel1Fields = level1.Level1Fields
            .Where(kvp => query.Level1Fields.Contains(kvp.Key))
            .ToDictionary();

        var decryptedLevel1Secret = encryption.Decrypt(level1.Secret.FromBase64String(), vaultKey);
        if (requiredLevel1Fields.Count != 0)
        {
            var onceDecryptedLevel1Fields = await vault.DecryptLabeledBatchAsync(query.UserId, requiredLevel1Fields);
            var level1FieldsKey = kdf.DeriveCompositeKey(vaultKey, decryptedLevel1Secret);

            foreach (var level1Field in onceDecryptedLevel1Fields)
            {
                response.Level1Fields[level1Field.Key] = encryption.Decrypt<string>(level1Field.Value, level1FieldsKey);
            }
        }

        if (query.Password is null || query.Level2Fields.Length <= 0)
        {
            var encryptedLevel1 = encryption.Encrypt(level1, level1Key);
            transaction.Put(storageKey, encryptedLevel1);
            transaction.Commit();
            return response;
        }

        level1.EnsureNotLockedOrThrow(transaction, dateTimeProvider);

        var securedPassword = kdf.DeriveKey(query.Password, query.UserId);
        var level2Key = kdf.DeriveCompositeKey(decryptedLevel1Secret, securedPassword);

        var level2 = level1.DecryptLevel2OrThrow<Level2>(
            storageKey,
            level1Key,
            level2Key,
            encryption,
            transaction,
            dateTimeProvider);

        if (level1.FailedAttempts > 0)
        {
            level1.FailedAttempts = 0;
        }

        var requiredLevel2Fields = level2.Level2Fields
            .Where(kvp => query.Level2Fields.Contains(kvp.Key))
            .ToDictionary();

        if (requiredLevel2Fields.Count != 0)
        {
            var onceDecryptedLevel2Fields = await vault.DecryptLabeledBatchAsync(query.UserId, requiredLevel2Fields);
            var decryptedLevel2Secret = encryption.Decrypt(level2.Secret.FromBase64String(), vaultKey);
            var level2FieldsKey = kdf.DeriveCompositeKey(decryptedLevel2Secret, securedPassword);

            foreach (var level2Field in onceDecryptedLevel2Fields)
            {
                response.Level2Fields[level2Field.Key] = encryption.Decrypt<string>(level2Field.Value, level2FieldsKey);
            }
        }

        var encryptedEntity = encryption.Encrypt(level1, level1Key);
        transaction.Put(storageKey, encryptedEntity);
        transaction.Commit();
        return response;
    }
}