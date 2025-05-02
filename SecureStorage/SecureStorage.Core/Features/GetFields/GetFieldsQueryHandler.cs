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
    public async Task<Dictionary<string, string?>> HandleAsync(GetFieldsQuery query)
    {
        var vaultKey = await vault.GetUserKeyAsync(query.UserId);
        var entityStorageKey = keyGenerator.GenerateKeyFromUserId(vaultKey, query.UserId);
        var level1Key = kdf.DeriveUserKey(vaultKey, query.UserId);

        var result = query.Fields.ToDictionary(x => x, _ => default(string));

        using var transaction = storage.BeginTransaction();

        var entity = transaction.GetAndDecryptOrThrow<SecureUser>(
            entityStorageKey,
            encryption,
            level1Key);
        entity.LastAccessedAt = dateTimeProvider.UtcNow;
        
        var level1Keys = query.Fields
            .Where(f => entity.Level1Fields.ContainsKey(f))
            .ToDictionary(f => entity.Level1Fields[f], f => f);

        if (level1Keys.Count != 0)
        {
            var level1Values = transaction.GetBatch(level1Keys.Keys);

            if (level1Values.Any(x => x.Value is not null))
            {
                var level1FieldsKey = kdf.DeriveCompositeKey(vaultKey, entity.Secret.ToBytes());
                foreach (var pair in level1Values.Where(x => x.Value is not null))
                {
                    result[level1Keys[pair.Key]] = encryption.TryDecrypt<string>(pair.Value!, level1FieldsKey);
                }
            }
        }

        var level2KeysQuery = query.Fields.Except(level1Keys.Values).ToList();
        if (query.Password is null || level2KeysQuery.Count <= 0)
            return result;

        entity.EnsureNotLockedOrThrow(transaction, dateTimeProvider);
        
        var level2Key = kdf.DeriveCompositeKey(vaultKey, entity.Secret.ToBytes(), query.Password);
        var level2 = entity.DecryptLevel2OrThrow<Level2>(
            entityStorageKey,
            level1Key,
            level2Key,
            encryption,
            transaction,
            dateTimeProvider);

        if (entity.FailedAttempts > 0)
        {
            entity.FailedAttempts = 0;
            var encryptedLevel1 = encryption.Encrypt(entity, level1Key);
            transaction.Put(entityStorageKey, encryptedLevel1);
        }

        var level2Keys = level2KeysQuery
            .Where(f => level2.Level2Fields.ContainsKey(f))
            .ToDictionary(f => level2.Level2Fields[f], f => f);

        if (level2Keys.Count != 0)
        {
            var level2Values = transaction.GetBatch(level2Keys.Keys);

            if (level2Values.Any(x => x.Value is not null))
            {
                var level2FieldsKey = kdf.DeriveCompositeKey(vaultKey, level2.Secret.ToBytes(), query.Password);

                foreach (var pair in level2Values.Where(x => x.Value is not null))
                {
                    result[level2Keys[pair.Key]] = encryption.TryDecrypt<string>(pair.Value!, level2FieldsKey);
                }
            }
        }

        transaction.Commit();
        return result;
    }
}