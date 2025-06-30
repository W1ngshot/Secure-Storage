using FastEndpoints;
using SecureStorage.API.Models.Base;

namespace SecureStorage.API.Processors;

public class UserIdExtractorPreProcessor : IGlobalPreProcessor
{
    public Task PreProcessAsync(IPreProcessorContext context, CancellationToken ct)
    {
        if (context.Request is not BaseRequest baseRequest)
            return Task.CompletedTask;

        if (!string.IsNullOrWhiteSpace(baseRequest.UserId))
        {
            context.HttpContext.Items["UserId"] = baseRequest.UserId;
        }

        return Task.CompletedTask;
    }
}