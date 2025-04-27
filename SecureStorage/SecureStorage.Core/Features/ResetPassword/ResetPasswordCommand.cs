namespace SecureStorage.Core.Features.ResetPassword;

public class ResetPasswordCommand
{
    public required string UserId { get; init; }
    public required string NewPassword { get; init; }
}