namespace SecureStorage.API.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        var requestId = Guid.NewGuid().ToString();
        context.Items["RequestId"] = requestId;

        logger.LogInformation("Request started. RequestId: {RequestId}, Path: {Path}", requestId, context.Request.Path);

        await next(context);

        var userId = context.Items.TryGetValue("UserId", out var userIdObj) ? userIdObj?.ToString() : null;

        logger.LogInformation(
            "Request completed. RequestId: {RequestId}, Path: {Path}, UserId: {UserId}, StatusCode: {StatusCode}",
            requestId, context.Request.Path, userId, context.Response.StatusCode);
    }
}