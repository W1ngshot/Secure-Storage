using SecureStorage.Core.Extensions;
using SecureStorage.Core.Interfaces.Persistence;
using SecureStorage.Core.Interfaces.Security;
using SecureStorage.Core.Interfaces.Utility;
using SecureStorage.Core.Interfaces.Vault;
using SecureStorage.Domain.Entities;

namespace SecureStorage.Core.Features.UpdateUser;

public class UpdateUserCommandHandler(
    IKeyValueStorage storage,
    IEncryptionService encryption,
    IKdfService kdf,
    IKeyGenerator keyGenerator,
    IKeyVault vault,
    IDateTimeProvider dateTimeProvider)
{
    public async Task HandleAsync(UpdateUserCommand command)
    {
        var vaultKey = await vault.GetUserKeyAsync(command.UserId);
        var storageKey = keyGenerator.GenerateKeyFromUserId(vaultKey, command.UserId);
        var level1Key = kdf.DeriveUserKey(vaultKey, command.UserId);

        using var transaction = storage.BeginTransaction();

        var level1 = transaction.GetAndDecryptOrThrow<Level1>(
            storageKey,
            encryption,
            level1Key);

        var decryptedLevel1Secret = encryption.Decrypt(level1.Secret.FromBase64String(), vaultKey);
        var level1FieldsKey = kdf.DeriveCompositeKey(vaultKey, decryptedLevel1Secret);

        var level1EncryptedList = command.Level1Updates.ToEncryptedList(encryption, level1FieldsKey);
        var level1DoubleKeyEncryptedList =
            await vault.EncryptLabeledBatchAsync(command.UserId, level1EncryptedList.ToDictionary());

        foreach (var level1Update in level1DoubleKeyEncryptedList)
        {
            level1.Level1Fields[level1Update.Key] = level1Update.Value;
        }

        if (command.Password is null || command.Level2Updates.Count <= 0)
        {
            var encryptedLevel1 = encryption.Encrypt(level1, level1Key);
            transaction.Put(storageKey, encryptedLevel1);
            transaction.Commit();
            return;
        }

        level1.EnsureNotLockedOrThrow(transaction, dateTimeProvider);
        var securedPassword = kdf.DeriveKey(command.Password, command.UserId);
        var level2Key = kdf.DeriveCompositeKey(decryptedLevel1Secret, securedPassword);

        var level2 = level1.DecryptLevel2OrThrow<Level2>(
            storageKey,
            level1Key,
            level2Key,
            encryption,
            transaction,
            dateTimeProvider);
        
        var decryptedLevel2Secret = encryption.Decrypt(level2.Secret.FromBase64String(), vaultKey);
        var level2FieldsKey = kdf.DeriveCompositeKey(decryptedLevel2Secret, securedPassword);

        var level2EncryptedList = command.Level2Updates.ToEncryptedList(encryption, level2FieldsKey);
        var level2DoubleKeyEncryptedList =
            await vault.EncryptLabeledBatchAsync(command.UserId, level2EncryptedList.ToDictionary());

        foreach (var level2Update in level2DoubleKeyEncryptedList)
        {
            level2.Level2Fields[level2Update.Key] = level2Update.Value;
        }
        
        level1.EncryptedLevel2 = encryption.Encrypt(level2, level2Key);
        var encryptedEntity = encryption.Encrypt(level1, level1Key);

        transaction.Put(storageKey, encryptedEntity);
        transaction.Commit();
    }
}