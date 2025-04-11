﻿namespace SecureStorage.Domain.Persistence;

public interface IStorageTransaction : IDisposable
{
    string? Get(string key);
    Dictionary<string, string?> GetBatch(IEnumerable<string> keys);
    void Put(string key, string value);
    void PutBatch(Dictionary<string, string> batch);
    void Delete(string key);
    void Commit();
}