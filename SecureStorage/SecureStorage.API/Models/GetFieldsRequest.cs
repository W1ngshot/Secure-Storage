namespace SecureStorage.API.Models;

public class GetFieldsRequest
{
    public required string UserId { get; init; }
    public string? Password { get; init; }
    public required string[] Level1Fields { get; init; }
    public required string[] Level2Fields { get; init; }
}