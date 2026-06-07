using System.Text.Json;
using ModelContextProtocol.Protocol;

namespace McpAzureContainerAppsDemo.Client.Rendering;

public static class ToolResultRenderer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public static string ToDisplayText(CallToolResult result)
    {
        if (result.IsError is true)
        {
            return "Tool returned an error: " + JsonSerializer.Serialize(result.Content, JsonOptions);
        }

        if (result.StructuredContent is not null)
        {
            return JsonSerializer.Serialize(result.StructuredContent, JsonOptions);
        }

        return string.Join(
            Environment.NewLine,
            result.Content.Select(RenderContentBlock));
    }

    private static string RenderContentBlock(ContentBlock block) =>
        block switch
        {
            TextContentBlock text => text.Text,
            _ => JsonSerializer.Serialize(block, JsonOptions)
        };
}
