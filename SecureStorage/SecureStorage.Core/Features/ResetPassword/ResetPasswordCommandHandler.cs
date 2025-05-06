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
        var vaultKey = await vault.GetUserKeyAsync(command.UserId);
        var storageKey = keyGenerator.GenerateKeyFromUserId(vaultKey, command.UserId);
        var level1Key = kdf.DeriveUserKey(vaultKey, command.UserId);

        using var transaction = storage.BeginTransaction();

        var level1 = transaction.GetAndDecryptOrThrow<Level1>(
            storageKey,
            encryption,
            level1Key);

        var securedPassword = kdf.DeriveKey(command.NewPassword, command.UserId);
        var decryptedLevel1Secret = encryption.Decrypt(level1.Secret.FromBase64String(), vaultKey);

        var newLevel2Key = kdf.DeriveCompositeKey(decryptedLevel1Secret, securedPassword);
        var newLevel2 = new Level2
        {
            Level2Fields = [],
            Secret = encryption.Encrypt(keyGenerator.GenerateRandomByteKey(), vaultKey).ToBase64String()
        };

        level1.EncryptedLevel2 = encryption.Encrypt(newLevel2, newLevel2Key);
        level1.LastAccessedAt = dateTimeProvider.UtcNow;
        level1.LockUntil = null;
        level1.FailedAttempts = 0;

        var encryptedLevel1 = encryption.Encrypt(level1, level1Key);

        transaction.Put(storageKey, encryptedLevel1);
        transaction.Commit();
    }
}