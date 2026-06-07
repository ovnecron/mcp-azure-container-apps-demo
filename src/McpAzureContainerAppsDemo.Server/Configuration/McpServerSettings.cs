using System.ComponentModel.DataAnnotations;

namespace McpAzureContainerAppsDemo.Server.Configuration;

public sealed class McpServerSettings
{
    public const string SectionName = "McpServer";

    [Required]
    public string Name { get; init; } = "MCP Azure Container Apps Demo";

    public string? ApiKey { get; init; }
}
