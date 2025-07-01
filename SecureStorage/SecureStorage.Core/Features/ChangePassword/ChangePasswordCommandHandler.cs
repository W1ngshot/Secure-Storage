using SecureStorage.Core.Extensions;
using SecureStorage.Core.Interfaces.Persistence;
using SecureStorage.Core.Interfaces.Security;
using SecureStorage.Core.Interfaces.Utility;
using SecureStorage.Core.Interfaces.Vault;
using SecureStorage.Domain.Entities;

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
        var storageKey = keyGenerator.GenerateKeyFromUserId(vaultKey, command.UserId);
        var level1Key = kdf.DeriveUserKey(vaultKey, command.UserId);

        var oldSecuredPassword = kdf.DeriveKey(command.OldPassword, command.UserId);
        var newSecuredPassword = kdf.DeriveKey(command.NewPassword, command.UserId);

        using var transaction = storage.BeginTransaction();

        var level1 = transaction.GetAndDecryptOrThrow<Level1>(
            storageKey,
            encryption,
            level1Key);

        level1.EnsureNotLockedOrThrow(transaction, dateTimeProvider);

        var decryptedLevel1Secret = encryption.Decrypt(level1.Secret.FromBase64String(), vaultKey);
        var oldLevel2Key = kdf.DeriveCompositeKey(decryptedLevel1Secret, oldSecuredPassword);
        var level2 = level1.DecryptLevel2OrThrow<Level2>(
            storageKey,
            level1Key,
            oldLevel2Key,
            encryption,
            transaction,
            dateTimeProvider);
        level1.FailedAttempts = 0;

        var decryptedLevel2Secret = encryption.Decrypt(level2.Secret.FromBase64String(), vaultKey);

        var oldLevel2FieldsKey = kdf.DeriveCompositeKey(decryptedLevel2Secret, oldSecuredPassword);
        var newLevel2FieldsKey = kdf.DeriveCompositeKey(decryptedLevel2Secret, newSecuredPassword);

        var onceDecryptedLevel2Fields = await vault.DecryptLabeledBatchAsync(command.UserId, level2.Level2Fields);
        var reEncryptedLevel2Fields = onceDecryptedLevel2Fields
            .Select(x =>
                new KeyValuePair<string, string>(x.Key, encryption.Decrypt<string>(x.Value, oldLevel2FieldsKey)))
            .ToEncryptedList(encryption, newLevel2FieldsKey);
        var level2DoubleKeyEncryptedList =
            await vault.EncryptLabeledBatchAsync(command.UserId, reEncryptedLevel2Fields.ToDictionary());

        foreach (var kvp in level2DoubleKeyEncryptedList)
        {
            level2.Level2Fields[kvp.Key] = kvp.Value;
        }
        
        var newLevel2Key = kdf.DeriveCompositeKey(decryptedLevel1Secret, newSecuredPassword);
        
        level1.EncryptedLevel2 = encryption.Encrypt(level2, newLevel2Key);
        
        var encryptedLevel1 = encryption.Encrypt(level1, level1Key);

        transaction.Put(storageKey, encryptedLevel1);
        transaction.Commit();
    }
}