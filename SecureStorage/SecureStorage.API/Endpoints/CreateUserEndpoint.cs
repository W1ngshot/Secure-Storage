using FastEndpoints;
using SecureStorage.Core.Features.CreateUser;

namespace SecureStorage.API.Endpoints;

public class CreateUserEndpoint(CreateUserCommandHandler commandHandler) : Endpoint<CreateUserCommand>
{
    public override void Configure()
    {
        Post("/users");
        AllowAnonymous();
        Summary(s => s.Summary = "Создание нового пользователя");
    }

    public override async Task HandleAsync(CreateUserCommand req, CancellationToken ct)
    {
        await commandHandler.HandleAsync(req);
        await SendAsync(new { status = "ok" }, cancellation: ct);
    }
}