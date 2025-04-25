using SecureStorage.Core.Extensions;
using SecureStorage.Domain.Entities;
using SecureStorage.Domain.Persistence;
using SecureStorage.Domain.Security;
using SecureStorage.Domain.Vault;

namespace SecureStorage.Core.Features.GetFields;

public class GetFieldsQueryHandler(
    IKeyValueStorage storage,
    IEncryptionService encryption,
    IKdfService kdf,
    IKeyGenerator keyGenerator,
    IKeyVault vault)
{
    public async Task<Dictionary<string, string?>> HandleAsync(GetFieldsQuery query)
    {
        var vaultKey = await vault.GetKeyForUserAsync(query.UserId);
        var entityKey = keyGenerator.GenerateKeyFromUserId(vaultKey, query.UserId);
        var level1Key = kdf.DeriveUserKey(vaultKey, query.UserId);

        var result = query.Fields.ToDictionary(x => x, _ => default(string));

        using var transaction = storage.BeginTransaction();

        var data = transaction.Get(entityKey);
        if (data is null) throw new Exception("User not found"); //TODO abort

        var entity = encryption.TryDecrypt<SecureUser>(data, level1Key);
        if (entity is null) throw new Exception("Invalid entity data"); //TODO abort

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

        var level2Key = kdf.DeriveCompositeKey(vaultKey, entity.Secret.ToBytes(), query.Password);
        var level2 = encryption.TryDecrypt<Level2>(entity.EncryptedLevel2, level2Key);

        if (level2 is null) throw new Exception("Invalid password"); //TODO abort

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

        return result;
    }
}