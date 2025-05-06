using FastEndpoints;
using SecureStorage.API.Models;
using SecureStorage.Core.Features.GetFields;

namespace SecureStorage.API.Endpoints;

public class GetFieldsEndpoint(GetFieldsQueryHandler queryHandler) : Endpoint<GetFieldsRequest, GetFieldsQueryResponse>
{
    public override void Configure()
    {
        Post("/users/fields");
        AllowAnonymous();
        Summary(s => s.Summary = "Получить выбранные поля пользователя");
    }

    public override async Task HandleAsync(GetFieldsRequest req, CancellationToken ct)
    {
        var result = await queryHandler.HandleAsync(new GetFieldsQuery
        {
            UserId = req.UserId,
            Level1Fields = req.Level1Fields,
            Level2Fields = req.Level2Fields,
            Password = req.Password
        });
        
        await SendAsync(result, cancellation: ct);
    }
}