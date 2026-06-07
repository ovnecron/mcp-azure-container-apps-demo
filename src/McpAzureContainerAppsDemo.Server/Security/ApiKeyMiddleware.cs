using McpAzureContainerAppsDemo.Server.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace McpAzureContainerAppsDemo.Server.Security;

public sealed class ApiKeyMiddleware(
    RequestDelegate next,
    IOptions<McpServerSettings> options,
    ILogger<ApiKeyMiddleware> logger)
{
    public const string ApiKeyHeaderName = "X-Api-Key";

    private readonly McpServerSettings _settings = options.Value;

    public async Task InvokeAsync(HttpContext context)
    {
        if (!RequiresApiKey(context.Request.Path, _settings.ApiKey))
        {
            await next(context);
            return;
        }

        string? providedKey = GetProvidedApiKey(context.Request);
        if (string.Equals(providedKey, _settings.ApiKey, StringComparison.Ordinal))
        {
            await next(context);
            return;
        }

        logger.LogWarning("Rejected unauthorized MCP request from {RemoteIpAddress}.", context.Connection.RemoteIpAddress);
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { error = "Missing or invalid API key." }, context.RequestAborted);
    }

    private static bool RequiresApiKey(PathString path, string? configuredApiKey) =>
        path.StartsWithSegments("/mcp", StringComparison.OrdinalIgnoreCase)
        && !string.IsNullOrWhiteSpace(configuredApiKey);

    private static string? GetProvidedApiKey(HttpRequest request)
    {
        if (request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyValues))
        {
            return apiKeyValues.ToString();
        }

        if (request.Headers.TryGetValue(HeaderNames.Authorization, out var authorizationValues))
        {
            const string bearerPrefix = "Bearer ";
            string authorization = authorizationValues.ToString();
            if (authorization.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return authorization[bearerPrefix.Length..].Trim();
            }
        }

        return null;
    }
}
