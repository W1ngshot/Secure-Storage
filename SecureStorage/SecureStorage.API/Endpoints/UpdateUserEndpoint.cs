using FastEndpoints;
using SecureStorage.Core.Features.UpdateUser;

namespace SecureStorage.API.Endpoints;

public class UpdateUserEndpoint(UpdateUserCommandHandler commandHandler) : Endpoint<UpdateUserCommand>
{
    public override void Configure()
    {
        Put("/users");
        AllowAnonymous();
        Summary(s => s.Summary = "Обновление данных пользователя");
    }

    public override async Task HandleAsync(UpdateUserCommand req, CancellationToken ct)
    {
        await commandHandler.HandleAsync(req);
        await SendAsync(new { status = "updated" }, cancellation: ct);
    }
}