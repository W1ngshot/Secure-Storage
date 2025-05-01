namespace SecureStorage.Domain.Entities;

public class SecureUser
{
    public required DateTime CreatedAt { get; init; }
    public DateTime? LastAccessedAt { get; set; }
    public int FailedAttempts { get; set; }
    public DateTime? LockUntil { get; set; }
    public required string Secret { get; init; }
    public Dictionary<string, string> Level1Fields { get; init; } = new(); // fieldName -> encryptedKey
    public required string EncryptedLevel2 { get; set; }
}