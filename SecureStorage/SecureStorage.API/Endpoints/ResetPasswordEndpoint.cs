using FastEndpoints;
using SecureStorage.API.Models;
using SecureStorage.Core.Features.ResetPassword;

namespace SecureStorage.API.Endpoints;

public class ResetPasswordEndpoint(ResetPasswordCommandHandler commandHandler) : Endpoint<ResetPasswordRequest>
{
    public override void Configure()
    {
        Post("/api/password/reset");
        AllowAnonymous();
        Summary(s => s.Summary = "Сброс пароля пользователя");
    }

    public override async Task HandleAsync(ResetPasswordRequest req, CancellationToken ct)
    {
        await commandHandler.HandleAsync(new ResetPasswordCommand
        {
            UserId = req.UserId,
            NewPassword = req.NewPassword
        });
        
        await SendNoContentAsync(cancellation: ct);
    }
}