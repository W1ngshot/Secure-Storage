namespace SecureStorage.API.Models;

public class ResetPasswordRequest
{
    public required string UserId { get; init; }
    public required string NewPassword { get; init; }
}