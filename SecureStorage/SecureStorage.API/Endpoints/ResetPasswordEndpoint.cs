using FastEndpoints;
using SecureStorage.Core.Features.ResetPassword;

namespace SecureStorage.API.Endpoints;

public class ResetPasswordEndpoint(ResetPasswordCommandHandler commandHandler) : Endpoint<ResetPasswordCommand>
{
    public override void Configure()
    {
        Post("/password/reset");
        AllowAnonymous();
        Summary(s => s.Summary = "Сброс пароля пользователя");
    }

    public override async Task HandleAsync(ResetPasswordCommand req, CancellationToken ct)
    {
        await commandHandler.HandleAsync(req);
        await SendAsync(new { status = "reset" }, cancellation: ct);
    }
}