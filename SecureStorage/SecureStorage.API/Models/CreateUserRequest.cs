using SecureStorage.API.Models.Base;

namespace SecureStorage.API.Models;

public class CreateUserRequest : BaseRequest
{
    public required Dictionary<string, string> Level1Fields { get; init; }
    public required Dictionary<string, string> Level2Fields { get; init; }
    public required string Password { get; init; }
}