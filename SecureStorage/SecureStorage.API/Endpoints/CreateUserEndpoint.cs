using FastEndpoints;
using SecureStorage.API.Models;
using SecureStorage.Core.Features.CreateUser;

namespace SecureStorage.API.Endpoints;

public class CreateUserEndpoint(CreateUserCommandHandler commandHandler) : Endpoint<CreateUserRequest>
{
    public override void Configure()
    {
        Post("/api/users");
        AllowAnonymous();
        Summary(s => s.Summary = "Создание нового пользователя");
    }

    public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
    {
        await commandHandler.HandleAsync(new CreateUserCommand
        {
            UserId = req.UserId,
            Level1Fields = req.Level1Fields,
            Level2Fields = req.Level2Fields,
            Password = req.Password
        });

        await SendNoContentAsync(cancellation: ct);
    }
}