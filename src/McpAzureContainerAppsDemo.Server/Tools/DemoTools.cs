using System.ComponentModel;
using McpAzureContainerAppsDemo.Server.Services;
using ModelContextProtocol.Server;

namespace McpAzureContainerAppsDemo.Server.Tools;

[McpServerToolType]
public sealed class DemoTools(
    CalculatorService calculator,
    ServerTimeService serverTime,
    ILogger<DemoTools> logger)
{
    [McpServerTool(Name = "echo", ReadOnly = true, Idempotent = true)]
    [Description("Returns the message supplied by the caller.")]
    public string Echo(
        [Description("Message to return.")]
        string message)
    {
        logger.LogInformation("Echo tool invoked.");
        return message;
    }

    [McpServerTool(Name = "add", ReadOnly = true, Idempotent = true)]
    [Description("Adds two 32-bit integers and returns the sum.")]
    public int Add(
        [Description("Left operand.")]
        int left,
        [Description("Right operand.")]
        int right) =>
        calculator.Add(left, right);

    [McpServerTool(Name = "server_time", ReadOnly = true, Idempotent = true)]
    [Description("Returns the current server time for an IANA or Windows time zone identifier.")]
    public Task<ServerTimeResult> GetServerTimeAsync(
        [Description("Time zone identifier. Examples: UTC, Europe/Berlin, America/Los_Angeles.")]
        string? timeZoneId = "UTC",
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(serverTime.GetTime(timeZoneId));
    }
}
