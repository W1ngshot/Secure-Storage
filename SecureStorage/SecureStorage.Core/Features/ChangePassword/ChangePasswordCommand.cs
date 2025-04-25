namespace SecureStorage.Core.Features.ChangePassword;

public class ChangePasswordCommand
{
    public required string UserId { get; init; }
    public required string OldPassword { get; init; }
    public required string NewPassword { get; init; }
}