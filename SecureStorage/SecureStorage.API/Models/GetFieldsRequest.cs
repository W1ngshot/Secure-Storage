namespace SecureStorage.API.Models;

public class GetFieldsRequest
{
    public required string UserId { get; init; }
    public string? Password { get; init; }
    public required string[] Fields { get; init; }
}