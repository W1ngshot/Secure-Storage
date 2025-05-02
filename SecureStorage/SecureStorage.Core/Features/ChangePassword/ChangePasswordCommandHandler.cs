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
        var vaultKey = await vault.GetUserKeyAsync(command.UserId);
        var entityStorageKey = keyGenerator.GenerateKeyFromUserId(vaultKey, command.UserId);
        var level1Key = kdf.DeriveUserKey(vaultKey, command.UserId);

        using var transaction = storage.BeginTransaction();

        var entity = transaction.GetAndDecryptOrThrow<SecureUser>(
            entityStorageKey,
            encryption,
            level1Key);
        entity.LastAccessedAt = dateTimeProvider.UtcNow;

        entity.EnsureNotLockedOrThrow(transaction, dateTimeProvider);

        var level2Key = kdf.DeriveCompositeKey(vaultKey, entity.Secret.ToBytes(), command.OldPassword);
        var level2 = entity.DecryptLevel2OrThrow<Level2>(
            entityStorageKey,
            level1Key,
            level2Key,
            encryption,
            transaction,
            dateTimeProvider);
        entity.FailedAttempts = 0;

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

        var encryptedLevel1 = encryption.Encrypt(entity, level1Key);

        var batch = updatedLevel2Fields
            .Select(x => new KeyValuePair<string, string>(x.StorageKey, x.Data))
            .Append(new KeyValuePair<string, string>(entityStorageKey, encryptedLevel1))
            .ToDictionary();

        transaction.PutBatch(batch);
        transaction.Commit();
    }
}