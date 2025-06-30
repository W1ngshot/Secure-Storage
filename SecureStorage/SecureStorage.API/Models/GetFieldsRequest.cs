using SecureStorage.API.Models.Base;

namespace SecureStorage.API.Models;

public class GetFieldsRequest : BaseRequest
{
    public string? Password { get; init; }
    public required string[] Level1Fields { get; init; }
    public required string[] Level2Fields { get; init; }
}