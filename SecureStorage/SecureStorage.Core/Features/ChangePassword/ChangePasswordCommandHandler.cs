using SecureStorage.Core.Extensions;
using SecureStorage.Domain.Entities;
using SecureStorage.Domain.Persistence;
using SecureStorage.Domain.Security;
using SecureStorage.Domain.Utility;
using SecureStorage.Domain.Vault;

namespace SecureStorage.Core.Features.ChangePassword;

public class ChangePasswordCommandHandler(
    IKeyValueStorage storage,
    IEncryptionService encryption,
    IKdfService kdf,
    IKeyVault vault,
    IKeyGenerator keyGenerator,
    IDateTimeProvider dateTimeProvider)
{
    public async Task HandleAsync(ChangePasswordCommand command)
    {
        var vaultKey = await vault.GetKeyForUserAsync(command.UserId);
        var entityKey = keyGenerator.GenerateKeyFromUserId(vaultKey, command.UserId);
        var level1Key = kdf.DeriveUserKey(vaultKey, command.UserId);

        using var transaction = storage.BeginTransaction();

        var data = transaction.Get(entityKey);
        if (data is null) throw new Exception("User not found"); //TODO abort

        var entity = encryption.TryDecrypt<SecureUser>(data, level1Key);
        if (entity is null) throw new Exception("Invalid entity data"); //TODO abort

        var level2Key = kdf.DeriveCompositeKey(vaultKey, entity.Secret.ToBytes(), command.OldPassword);
        var level2 = encryption.TryDecrypt<Level2>(entity.EncryptedLevel2, level2Key);

        if (level2 is null) throw new Exception("Invalid password"); //TODO abort

        var level2Keys = level2.Level2Fields.ToDictionary(pair => pair.Value, pair => pair.Key);
        var level2FieldsKey = kdf.DeriveCompositeKey(vaultKey, level2.Secret.ToBytes(), command.OldPassword);
        var newLevel2FieldsKey = kdf.DeriveCompositeKey(vaultKey, level2.Secret.ToBytes(), command.NewPassword);

        var updatedLevel2Fields = transaction.GetBatch(level2Keys.Keys)
            .Select(x => (StorageKey: x.Key, Data: encryption.TryDecrypt<string>(x.Value!, level2FieldsKey)))
            .Where(x => !string.IsNullOrEmpty(x.Data))
            .Select(x => (level2Keys[x.StorageKey], x.StorageKey, x.Data!))
            .ToUpdatedEncryptedList(encryption, newLevel2FieldsKey);

        var newLevel2Key = kdf.DeriveCompositeKey(vaultKey, entity.Secret.ToBytes(), command.NewPassword);
        entity.EncryptedLevel2 = encryption.Encrypt(level2, newLevel2Key);
        entity.LastAccessedAt = dateTimeProvider.UtcNow;

        var encryptedLevel1 = encryption.Encrypt(entity, level1Key);

        var batch = updatedLevel2Fields
            .Select(x => new KeyValuePair<string, string>(x.StorageKey, x.Data))
            .Append(new KeyValuePair<string, string>(entityKey, encryptedLevel1))
            .ToDictionary();

        transaction.PutBatch(batch);
        transaction.Commit();
    }
}