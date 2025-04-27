using SecureStorage.Core.Extensions;
using SecureStorage.Domain.Entities;
using SecureStorage.Domain.Persistence;
using SecureStorage.Domain.Security;
using SecureStorage.Domain.Utility;
using SecureStorage.Domain.Vault;

namespace SecureStorage.Core.Features.ResetPassword;

public class ResetPasswordCommandHandler(
    IKeyValueStorage storage,
    IEncryptionService encryption,
    IKdfService kdf,
    IKeyGenerator keyGenerator,
    IKeyVault vault,
    IDateTimeProvider dateTimeProvider)
{
    public async Task HandleAsync(ResetPasswordCommand command)
    {
        var vaultKey = await vault.GetKeyForUserAsync(command.UserId);
        var entityStorageKey = keyGenerator.GenerateKeyFromUserId(vaultKey, command.UserId);
        var level1Key = kdf.DeriveUserKey(vaultKey, command.UserId);

        using var transaction = storage.BeginTransaction();

        var entity = transaction.GetAndDecryptOrThrow<SecureUser>(
            entityStorageKey,
            encryption,
            level1Key);

        var newLevel2Key = kdf.DeriveCompositeKey(vaultKey, entity.Secret.ToBytes(), command.NewPassword);
        var newLevel2 = new Level2
        {
            Level2Fields = [],
            Secret = keyGenerator.GenerateRandomKey()
        };

        entity.EncryptedLevel2 = encryption.Encrypt(newLevel2, newLevel2Key);
        entity.LastAccessedAt = dateTimeProvider.UtcNow;
        entity.LockUntil = null;
        entity.FailedAttempts = 0;

        var encryptedLevel1 = encryption.Encrypt(entity, level1Key);

        transaction.Put(entityStorageKey, encryptedLevel1);
        transaction.Commit();
    }
}