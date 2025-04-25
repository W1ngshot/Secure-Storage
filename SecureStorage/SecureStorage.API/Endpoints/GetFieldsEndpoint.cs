using FastEndpoints;
using SecureStorage.Core.Features.GetFields;

namespace SecureStorage.API.Endpoints;

public class GetFieldsEndpoint(GetFieldsQueryHandler queryHandler) : Endpoint<GetFieldsQuery, Dictionary<string, string?>>
{
    public override void Configure()
    {
        Post("/users/fields");
        AllowAnonymous();
        Summary(s => s.Summary = "Получить выбранные поля пользователя");
    }

    public override async Task HandleAsync(GetFieldsQuery req, CancellationToken ct)
    {
        var result = await queryHandler.HandleAsync(req);
        await SendAsync(result, cancellation: ct);
    }
}