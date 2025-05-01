namespace SecureStorage.API.Models;

public class CreateUserRequest
{
    public required string UserId { get; init; }
    public required Dictionary<string, string> Level1Fields { get; init; }
    public required Dictionary<string, string> Level2Fields { get; init; }
    public required string Password { get; init; }
}