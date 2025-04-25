using FastEndpoints;
using SecureStorage.Core.Features.ChangePassword;

namespace SecureStorage.API.Endpoints;

public class ChangePasswordEndpoint(ChangePasswordCommandHandler commandHandler) : Endpoint<ChangePasswordCommand>
{
    public override void Configure()
    {
        Post("/password/change");
        AllowAnonymous();
        Summary(s => s.Summary = "Смена пароля пользователя");
    }

    public override async Task HandleAsync(ChangePasswordCommand req, CancellationToken ct)
    {
        await commandHandler.HandleAsync(req);
        await SendAsync(new { status = "changed" }, cancellation: ct);
    }
}