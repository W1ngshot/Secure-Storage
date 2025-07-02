using FastEndpoints;
using SecureStorage.API.Models;
using SecureStorage.Core.Features.UpdateUser;

namespace SecureStorage.API.Endpoints;

public class UpdateUserEndpoint(UpdateUserCommandHandler commandHandler) : Endpoint<UpdateUserRequest>
{
    public override void Configure()
    {
        Put("/api/users");
        AllowAnonymous();
        Summary(s => s.Summary = "Обновление данных пользователя");
    }

    public override async Task HandleAsync(UpdateUserRequest req, CancellationToken ct)
    {
        await commandHandler.HandleAsync(new UpdateUserCommand
        {
            UserId = req.UserId,
            Level1Updates = req.Level1Updates,
            Level2Updates = req.Level2Updates,
            Password = req.Password
        });

        await SendNoContentAsync(cancellation: ct);
    }
}