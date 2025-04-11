using RocksDbSharp;
using SecureStorage.Domain.Persistence;

namespace SecureStorage.Infrastructure.Persistence;

public class StorageTransaction(Transaction txn) : IStorageTransaction
{
    public string? Get(string key) => txn.Get(key);

    public Dictionary<string, string?> GetBatch(IEnumerable<string> keys)
    {
        var keyArray = keys.ToArray();
        var values = txn.MultiGet(keyArray)?.ToDictionary(k => k.Key, v => v.Value);

        return keyArray.ToDictionary(key => key, key => values?.GetValueOrDefault(key));
    }

    public void Put(string key, string value) => txn.Put(key, value);

    public void PutBatch(Dictionary<string, string> batch)
    {
        foreach (var (key, value) in batch)
            txn.Put(key, value);
    }

    public void Delete(string key) => txn.Remove(key);

    public void Commit() => txn.Commit();

    public void Dispose() => txn.Dispose();
}