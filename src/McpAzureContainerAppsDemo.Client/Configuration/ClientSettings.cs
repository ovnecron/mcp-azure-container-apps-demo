namespace McpAzureContainerAppsDemo.Client.Configuration;

public sealed class ClientSettings
{
    private const string ApiKeyHeaderName = "X-Api-Key";

    public Uri ServerUrl { get; init; } = new("http://localhost:8080/mcp");

    public string? ApiKey { get; init; }

    public string TimeZoneId { get; init; } = "UTC";

    public static ClientSettings FromEnvironment(string[] args)
    {
        string? serverUrl = GetOption(args, "--url") ?? Environment.GetEnvironmentVariable("MCP_SERVER_URL");
        string? apiKey = GetOption(args, "--api-key") ?? Environment.GetEnvironmentVariable("MCP_API_KEY");
        string timeZoneId = GetOption(args, "--time-zone")
            ?? Environment.GetEnvironmentVariable("MCP_TIME_ZONE")
            ?? "UTC";

        return new()
        {
            ServerUrl = string.IsNullOrWhiteSpace(serverUrl)
                ? new Uri("http://localhost:8080/mcp")
                : new Uri(serverUrl, UriKind.Absolute),
            ApiKey = string.IsNullOrWhiteSpace(apiKey) ? null : apiKey,
            TimeZoneId = timeZoneId
        };
    }

    public Dictionary<string, string>? CreateHeaders()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            return null;
        }

        return new(StringComparer.OrdinalIgnoreCase)
        {
            [ApiKeyHeaderName] = ApiKey
        };
    }

    private static string? GetOption(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return null;
    }
}
