using FastEndpoints;
using SecureStorage.API.Models;
using SecureStorage.Core.Features.ChangePassword;

namespace SecureStorage.API.Endpoints;

public class ChangePasswordEndpoint(ChangePasswordCommandHandler commandHandler) : Endpoint<ChangePasswordRequest>
{
    public override async Task HandleAsync(ChangePasswordRequest req, CancellationToken ct)
    {
        await commandHandler.HandleAsync(new ChangePasswordCommand
        {
            UserId = req.UserId,
            OldPassword = req.OldPassword,
            NewPassword = req.NewPassword
        });

        await SendNoContentAsync(cancellation: ct);
    }

    public override void Configure()
    {
        Post("/password/change");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Change password";
            s.Description = "Let user change password by passing old password and new password";
        });

        Description(b =>
        {
            b.Produces(204);
            b.ProducesProblemDetails();
            b.Produces(403);
            b.Produces(500);
        });
    }
}