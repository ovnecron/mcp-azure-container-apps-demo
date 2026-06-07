using McpAzureContainerAppsDemo.Server.Configuration;
using McpAzureContainerAppsDemo.Server.Security;
using McpAzureContainerAppsDemo.Server.Services;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<McpServerSettings>()
    .Bind(builder.Configuration.GetSection(McpServerSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<CalculatorService>();
builder.Services.AddSingleton<ServerTimeService>();

builder.Services
    .AddMcpServer(options =>
    {
        options.ServerInfo = new()
        {
            Name = "mcp-azure-container-apps-demo",
            Version = "1.0.0"
        };
    })
    .WithHttpTransport(options => options.Stateless = true)
    .WithToolsFromAssembly();

var app = builder.Build();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapGet("/", (IOptions<McpServerSettings> options) => Results.Ok(new
{
    service = options.Value.Name,
    mcpEndpoint = "/mcp",
    health = "/health"
}));

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    utc = TimeProvider.System.GetUtcNow()
}));

app.MapMcp("/mcp");

app.Run();

public partial class Program;
