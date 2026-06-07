namespace McpAzureContainerAppsDemo.Server.Services;

public sealed record ServerTimeResult(
    string TimeZoneId,
    DateTimeOffset UtcNow,
    DateTimeOffset LocalTime);
