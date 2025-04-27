using SecureStorage.Domain.Exceptions;
using SecureStorage.Domain.Persistence;
using SecureStorage.Domain.Security;

namespace SecureStorage.Core.Extensions;

public static class EncryptionExtensions
{
    public static List<(string Key, string StorageKey, string Data)> ToEncryptedList(
        this IEnumerable<KeyValuePair<string, string>> fields,
        IKeyGenerator keyGenerator,
        IEncryptionService encryption,
        byte[] levelKey)
    {
        return fields
            .Select(field => (
                Key: field.Key,
                StorageKey: keyGenerator.GenerateRandomKey(),
                Data: encryption.Encrypt(field.Value, levelKey)))
            .ToList();
    }

    public static List<(string Key, string StorageKey, string Data)> ToUpdatedEncryptedList(
        this IEnumerable<(string Key, string StorageKey, string NewData)> levelFields,
        IEncryptionService encryption,
        byte[] levelKey)
    {
        return levelFields
            .Select(field => (
                field.Key,
                field.StorageKey,
                Data: encryption.Encrypt(field.NewData, levelKey)))
            .ToList();
    }

    public static T GetAndDecryptOrThrow<T>(
        this IStorageTransaction transaction,
        string key,
        IEncryptionService encryption,
        byte[] decryptKey)
    {
        var data = transaction.Get(key);
        if (data is null)
        {
            transaction.Rollback();
            throw new NotFoundException(nameof(T));
        }

        var entity = encryption.TryDecrypt<T>(data, decryptKey);
        if (entity is not null)
            return entity;

        transaction.Rollback();
        throw new CorruptedEntityException();
    }
}