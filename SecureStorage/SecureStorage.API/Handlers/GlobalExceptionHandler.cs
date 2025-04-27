using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SecureStorage.Domain.Exceptions;

namespace SecureStorage.API.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        context.Response.ContentType = "application/problem+json";

        if (exception is DomainException domainException)
        {
            context.Response.StatusCode = domainException.StatusCode;

            var problemDetails = new ProblemDetails
            {
                Status = domainException.StatusCode,
                Type = $"https://httpstatuses.com/{domainException.StatusCode}",
                Title = domainException.ErrorCode,
                Detail = domainException.PlaceholderData.Count > 0
                    ? System.Text.Json.JsonSerializer.Serialize(domainException.PlaceholderData)
                    : null,
                Instance = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }

        context.Response.StatusCode = 500;

        var internalProblem = new ProblemDetails
        {
            Status = 500,
            Type = "https://httpstatuses.com/500",
            Title = "error.internal",
            Detail = "Internal server error",
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(internalProblem, cancellationToken);

        return true;
    }
}