using SecureStorage.Core.Extensions;
using SecureStorage.Core.Interfaces.Persistence;
using SecureStorage.Core.Interfaces.Security;
using SecureStorage.Core.Interfaces.Utility;
using SecureStorage.Core.Interfaces.Vault;
using SecureStorage.Domain.Entities;

namespace SecureStorage.Core.Features.CreateUser;

public class CreateUserCommandHandler(
    IKeyValueStorage storage,
    IEncryptionService encryption,
    IKdfService kdf,
    IKeyGenerator keyGenerator,
    IKeyVault vault,
    IDateTimeProvider dateTimeProvider)
{
    public async Task HandleAsync(CreateUserCommand command)
    {
        var securedPassword = kdf.DeriveKey(command.Password, command.UserId);

        var vaultKey = keyGenerator.GenerateRandomByteKey();
        await vault.CreateUserKeyAsync(command.UserId, vaultKey);

        var storageKey = keyGenerator.GenerateKeyFromUserId(vaultKey, command.UserId);

        var level1Secret = keyGenerator.GenerateRandomByteKey();
        var level2Secret = keyGenerator.GenerateRandomByteKey();

        var level1Key = kdf.DeriveUserKey(vaultKey, command.UserId);
        var level1FieldsKey = kdf.DeriveCompositeKey(vaultKey, level1Secret);
        var level2Key = kdf.DeriveCompositeKey(level1Secret, securedPassword);
        var level2FieldsKey = kdf.DeriveCompositeKey(level2Secret, securedPassword);

        var level2EncryptedList = command.Level2Fields.ToEncryptedList(encryption, level2FieldsKey);
        var level2DoubleKeyEncryptedList =
            await vault.EncryptLabeledBatchAsync(command.UserId, level2EncryptedList.ToDictionary());

        var level2 = new Level2
        {
            Secret = encryption.Encrypt(level2Secret, vaultKey).ToBase64String(),
            Level2Fields = level2DoubleKeyEncryptedList
        };

        var level1EncryptedList = command.Level1Fields.ToEncryptedList(encryption, level1FieldsKey);
        var level1DoubleKeyEncryptedList =
            await vault.EncryptLabeledBatchAsync(command.UserId, level1EncryptedList.ToDictionary());

        var level1 = new Level1
        {
            CreatedAt = dateTimeProvider.UtcNow,
            FailedAttempts = 0,
            LockUntil = null,
            Secret = encryption.Encrypt(level1Secret, vaultKey).ToBase64String(),
            Level1Fields = level1DoubleKeyEncryptedList,
            EncryptedLevel2 = encryption.Encrypt(level2, level2Key),
        };

        var encryptedLevel1 = encryption.Encrypt(level1, level1Key);

        using var transaction = storage.BeginTransaction();
        transaction.Put(storageKey, encryptedLevel1);
        transaction.Commit();
    }
}