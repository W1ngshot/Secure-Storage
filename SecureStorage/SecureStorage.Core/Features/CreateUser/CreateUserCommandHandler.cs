using SecureStorage.Core.Extensions;
using SecureStorage.Domain.Entities;
using SecureStorage.Domain.Persistence;
using SecureStorage.Domain.Security;
using SecureStorage.Domain.Utility;
using SecureStorage.Domain.Vault;

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
        var vaultKey = keyGenerator.GenerateRandomByteKey();
        await vault.CreateUserKeyAsync(command.UserId, vaultKey);
        
        var entityKey = keyGenerator.GenerateKeyFromUserId(vaultKey, command.UserId);
        var level1Secret = keyGenerator.GenerateRandomKey();
        var level2Secret = keyGenerator.GenerateRandomKey();

        var level1Key = kdf.DeriveUserKey(vaultKey, command.UserId);
        var level1FieldsKey = kdf.DeriveCompositeKey(vaultKey, level1Secret.ToBytes());
        var level2Key = kdf.DeriveCompositeKey(vaultKey, level1Secret.ToBytes(), command.Password);
        var level2FieldsKey = kdf.DeriveCompositeKey(vaultKey, level2Secret.ToBytes(), command.Password);

        var level2EncryptedList = command.Level2Fields.ToEncryptedList(keyGenerator, encryption, level2FieldsKey);

        var level2 = new Level2
        {
            Secret = level2Secret,
            Level2Fields = level2EncryptedList.ToDictionary(x => x.Key, x => x.StorageKey)
        };

        var level1FieldValues = command.Level1Fields.ToEncryptedList(keyGenerator, encryption, level1FieldsKey);

        var entity = new SecureUser
        {
            CreatedAt = dateTimeProvider.UtcNow,
            LastAccessedAt = null,
            FailedAttempts = 0,
            LockUntil = null,
            Secret = level1Secret,
            Level1Fields = level1FieldValues.ToDictionary(x => x.Key, x => x.StorageKey),
            EncryptedLevel2 = encryption.Encrypt(level2, level2Key),
        };

        var encryptedLevel1 = encryption.Encrypt(entity, level1Key);

        var batch = level1FieldValues
            .Concat(level2EncryptedList)
            .Select(x => new KeyValuePair<string, string>(x.StorageKey, x.Data))
            .Append(new KeyValuePair<string, string>(entityKey, encryptedLevel1))
            .ToDictionary(x => x.Key, x => x.Value);

        using var transaction = storage.BeginTransaction();
        transaction.PutBatch(batch);
        transaction.Commit();
    }
}