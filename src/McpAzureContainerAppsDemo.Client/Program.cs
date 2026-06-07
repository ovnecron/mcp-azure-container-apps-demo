using McpAzureContainerAppsDemo.Client.Configuration;
using McpAzureContainerAppsDemo.Client.Rendering;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

using ILoggerFactory loggerFactory = LoggerFactory.Create(logging =>
{
    logging.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss ";
    });
});

var settings = ClientSettings.FromEnvironment(args);
var transportOptions = new HttpClientTransportOptions
{
    Name = "mcp-azure-container-apps-demo-client",
    Endpoint = settings.ServerUrl,
    TransportMode = HttpTransportMode.StreamableHttp,
    AdditionalHeaders = settings.CreateHeaders()
};

await using var transport = new HttpClientTransport(transportOptions, loggerFactory);
await using McpClient client = await McpClient.CreateAsync(transport, loggerFactory: loggerFactory);

IList<McpClientTool> tools = await client.ListToolsAsync();
Console.WriteLine($"Connected to {settings.ServerUrl}");
Console.WriteLine("Available tools:");
foreach (McpClientTool tool in tools.OrderBy(tool => tool.Name, StringComparer.Ordinal))
{
    Console.WriteLine($"- {tool.Name}: {tool.Description}");
}

CallToolResult addResult = await client.CallToolAsync(
    "add",
    new Dictionary<string, object?>
    {
        ["left"] = 40,
        ["right"] = 2
    });

Console.WriteLine();
Console.WriteLine("add(left: 40, right: 2)");
Console.WriteLine(ToolResultRenderer.ToDisplayText(addResult));

CallToolResult timeResult = await client.CallToolAsync(
    "server_time",
    new Dictionary<string, object?>
    {
        ["timeZoneId"] = settings.TimeZoneId
    });

Console.WriteLine();
Console.WriteLine($"server_time(timeZoneId: {settings.TimeZoneId})");
Console.WriteLine(ToolResultRenderer.ToDisplayText(timeResult));
