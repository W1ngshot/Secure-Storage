using SecureStorage.API.Models.Base;

namespace SecureStorage.API.Models;

public class ResetPasswordRequest : BaseRequest
{
    public required string NewPassword { get; init; }
}