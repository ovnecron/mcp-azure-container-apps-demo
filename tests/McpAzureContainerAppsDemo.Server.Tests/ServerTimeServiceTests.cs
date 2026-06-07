using McpAzureContainerAppsDemo.Server.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace McpAzureContainerAppsDemo.Server.Tests;

public sealed class ServerTimeServiceTests
{
    [Fact]
    public void GetTime_ReturnsUtcWhenNoTimezoneIsProvided()
    {
        var fixedUtcNow = new DateTimeOffset(2026, 6, 2, 12, 0, 0, TimeSpan.Zero);
        var service = new ServerTimeService(
            new FakeTimeProvider(fixedUtcNow),
            NullLogger<ServerTimeService>.Instance);

        ServerTimeResult result = service.GetTime(null);

        Assert.Equal("UTC", result.TimeZoneId);
        Assert.Equal(fixedUtcNow, result.UtcNow);
        Assert.Equal(fixedUtcNow, result.LocalTime);
    }

    [Fact]
    public void GetTime_ThrowsForUnknownTimezone()
    {
        var service = new ServerTimeService(
            TimeProvider.System,
            NullLogger<ServerTimeService>.Instance);

        Assert.Throws<TimeZoneNotFoundException>(() => service.GetTime("Not/A-Time-Zone"));
    }

    private sealed class FakeTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
