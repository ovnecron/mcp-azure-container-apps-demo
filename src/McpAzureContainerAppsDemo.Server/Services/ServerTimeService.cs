namespace McpAzureContainerAppsDemo.Server.Services;

public sealed class ServerTimeService(TimeProvider timeProvider, ILogger<ServerTimeService> logger)
{
    public ServerTimeResult GetTime(string? timeZoneId)
    {
        string resolvedTimeZoneId = string.IsNullOrWhiteSpace(timeZoneId)
            ? "UTC"
            : timeZoneId;

        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(resolvedTimeZoneId);
        DateTimeOffset utcNow = timeProvider.GetUtcNow();
        DateTimeOffset localTime = TimeZoneInfo.ConvertTime(utcNow, timeZone);

        logger.LogDebug("Resolved server time for timezone {TimeZoneId}.", timeZone.Id);
        return new(timeZone.Id, utcNow, localTime);
    }
}
