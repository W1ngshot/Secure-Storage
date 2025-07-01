using SecureStorage.Core.Interfaces.Persistence;
using SecureStorage.Core.Interfaces.Security;
using SecureStorage.Core.Interfaces.Utility;
using SecureStorage.Domain.Entities;
using SecureStorage.Domain.Exceptions;

namespace SecureStorage.Core.Extensions;

public static class EntityExtensions
{
    public static void EnsureNotLockedOrThrow(
        this Level1 entity,
        IStorageTransaction transaction,
        IDateTimeProvider dateTimeProvider)
    {
        if (!entity.LockUntil.HasValue || entity.LockUntil.Value <= dateTimeProvider.UtcNow)
            return;

        transaction.Commit();
        throw new LockedException(entity.LockUntil.Value);
    }

    public static T DecryptLevel2OrThrow<T>(
        this Level1 entity,
        string storageKey,
        byte[] level1Key,
        byte[] level2Key,
        IEncryptionService encryption,
        IStorageTransaction transaction,
        IDateTimeProvider dateTimeProvider)
    {
        var decrypted = encryption.TryDecrypt<T>(entity.EncryptedLevel2, level2Key);

        if (decrypted is not null)
            return decrypted;

        var failedAttempts = ++entity.FailedAttempts;
        DateTime? lockedUntil = null;

        if (failedAttempts >= 3)
            lockedUntil = dateTimeProvider.UtcNow.AddMinutes(5);

        entity.LockUntil = lockedUntil;
        transaction.Put(storageKey, encryption.Encrypt(entity, level1Key));
        transaction.Commit();

        if (lockedUntil is not null)
            throw new LockedException(lockedUntil.Value);

        throw new InvalidPasswordException(failedAttempts);
    }
}