using SecureStorage.Core.Extensions;
using SecureStorage.Domain.Entities;
using SecureStorage.Domain.Persistence;
using SecureStorage.Domain.Security;
using SecureStorage.Domain.Vault;

namespace SecureStorage.Core.Features.UpdateUser;

public class UpdateUserCommandHandler(
    IKeyValueStorage storage,
    IEncryptionService encryption,
    IKdfService kdf,
    IKeyGenerator keyGenerator,
    IKeyVault vault)
{
    public async Task HandleAsync(UpdateUserCommand command)
    {
        var vaultKey = await vault.GetKeyForUserAsync(command.UserId);
        var entityKey = keyGenerator.GenerateKeyFromUserId(vaultKey, command.UserId);
        var level1Key = kdf.DeriveUserKey(vaultKey, command.UserId);

        using var transaction = storage.BeginTransaction();

        var data = transaction.Get(entityKey);
        if (data is null) throw new Exception("User not found"); //TODO abort

        var entity = encryption.TryDecrypt<SecureUser>(data, level1Key);
        if (entity is null) throw new Exception("Invalid entity data"); //TODO abort

        var level1FieldsKey = kdf.DeriveCompositeKey(vaultKey, entity.Secret.ToBytes());

        var level1UpdatedFieldValues = entity.Level1Fields
            .Where(f => command.Level1Updates.ContainsKey(f.Key))
            .Select(f => (f.Key, f.Value, command.Level1Updates[f.Key]))
            .ToUpdatedEncryptedList(encryption, level1FieldsKey);

        var level1NewFieldValues = command.Level1Updates
            .ExceptBy(level1UpdatedFieldValues.Select(f => f.Key), f => f.Key)
            .ToEncryptedList(keyGenerator, encryption, level1FieldsKey);
        //TODO change last accessed at
        entity.Level1Fields.AddRange(level1NewFieldValues
            .Select(f => new KeyValuePair<string, string>(f.Key, f.StorageKey)));

        if (command.Password is null || command.Level2Updates.Count == 0)
        {
            var encryptedEntity = encryption.Encrypt(entity, level1Key);

            var level1Batch = level1UpdatedFieldValues
                .Concat(level1NewFieldValues)
                .Select(x => new KeyValuePair<string, string>(x.StorageKey, x.Data))
                .Append(new KeyValuePair<string, string>(entityKey, encryptedEntity))
                .ToDictionary();

            transaction.PutBatch(level1Batch);
            transaction.Commit();
            return;
        }

        var level2Key = kdf.DeriveCompositeKey(vaultKey, entity.Secret.ToBytes(), command.Password);
        var level2 = encryption.TryDecrypt<Level2>(entity.EncryptedLevel2, level2Key);
        if (level2 is null) throw new Exception("Invalid password"); //TODO abort

        var level2FieldsKey = kdf.DeriveCompositeKey(vaultKey, level2.Secret.ToBytes(), command.Password);

        var level2UpdatedFieldValues = level2.Level2Fields
            .Where(f => command.Level2Updates.ContainsKey(f.Key))
            .Select(f => (f.Key, f.Value, command.Level2Updates[f.Key]))
            .ToUpdatedEncryptedList(encryption, level2FieldsKey);

        var level2NewFieldValues = command.Level2Updates
            .ExceptBy(level2UpdatedFieldValues.Select(f => f.Key), f => f.Key)
            .ToEncryptedList(keyGenerator, encryption, level2FieldsKey);

        if (level2NewFieldValues.Count != 0)
        {
            level2.Level2Fields.AddRange(level2NewFieldValues
                .Select(f => new KeyValuePair<string, string>(f.Key, f.StorageKey)));

            entity.EncryptedLevel2 = encryption.Encrypt(level2, level2Key);
        }

        var encryptedLevel1 = encryption.Encrypt(entity, level1Key);

        var batch = level1UpdatedFieldValues
            .Concat(level1NewFieldValues)
            .Concat(level2UpdatedFieldValues)
            .Concat(level2NewFieldValues)
            .Select(x => new KeyValuePair<string, string>(x.StorageKey, x.Data))
            .Append(new KeyValuePair<string, string>(entityKey, encryptedLevel1))
            .ToDictionary();

        transaction.PutBatch(batch);
        transaction.Commit();
    }
}