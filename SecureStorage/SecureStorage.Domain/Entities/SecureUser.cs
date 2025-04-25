namespace SecureStorage.Domain.Entities;

public class SecureUser
{
    public required string UserId { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? LastAccessedAt { get; set; }
    public required string Secret { get; set; }
    public Dictionary<string, string> Level1Fields { get; set; } = new(); // fieldName -> encryptedKey
    public required string EncryptedLevel2 { get; set; }
}
