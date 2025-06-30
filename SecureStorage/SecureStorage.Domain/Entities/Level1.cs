namespace SecureStorage.Domain.Entities;

public class Level1
{
    public required DateTime CreatedAt { get; init; }
    public int FailedAttempts { get; set; }
    public DateTime? LockUntil { get; set; }
    public required string Secret { get; init; }
    public Dictionary<string, string> Level1Fields { get; init; } = new();
    public required string EncryptedLevel2 { get; set; }
}