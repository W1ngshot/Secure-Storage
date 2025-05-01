using FastEndpoints;
using SecureStorage.API.Models;
using SecureStorage.Core.Features.GetFields;

namespace SecureStorage.API.Endpoints;

public class GetFieldsEndpoint(GetFieldsQueryHandler queryHandler) : Endpoint<GetFieldsRequest, Dictionary<string, string?>>
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
            Fields = req.Fields,
            Password = req.Password
        });
        
        await SendAsync(result, cancellation: ct);
    }
}