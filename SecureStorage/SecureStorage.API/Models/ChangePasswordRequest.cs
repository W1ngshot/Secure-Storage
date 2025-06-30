using SecureStorage.API.Models.Base;

namespace SecureStorage.API.Models;

public class ChangePasswordRequest : BaseRequest
{
    public required string OldPassword { get; init; }
    public required string NewPassword { get; init; }
}