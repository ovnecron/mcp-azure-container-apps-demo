# MCP Server on Azure Container Apps with C# and .NET

This sample shows how to build an MCP server from zero with the official C# SDK, run it locally over Streamable HTTP, containerize it, deploy it to Azure Container Apps, and call it from a C# MCP client.

Accompanying blog post: [Build and deploy an MCP server with .NET and Azure Container Apps](http://lukaswalter.dev/posts/mcp-server-dotnet-aca/)

The server exposes three read-only tools:

- `echo`: returns a caller-provided message.
- `add`: adds two integers with checked overflow behavior.
- `server_time`: returns server time for a supplied time zone.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Docker, for local container verification
- Azure CLI, for Azure Container Apps deployment
- An Azure subscription
- Optional: Node.js for the MCP Inspector

## Project Layout

```text
McpAzureContainerAppsDemo/
  McpAzureContainerAppsDemo.slnx
  Directory.Build.props
  Directory.Packages.props
  Dockerfile
  src/
    McpAzureContainerAppsDemo.Server/
      Program.cs
      Configuration/
      Security/
      Services/
      Tools/
    McpAzureContainerAppsDemo.Client/
      Program.cs
      Configuration/
      Rendering/
  tests/
    McpAzureContainerAppsDemo.Server.Tests/
```

## Create the Project from Zero

These are the core commands used to create the project shape:

```bash
mkdir McpAzureContainerAppsDemo
cd McpAzureContainerAppsDemo

dotnet new sln -n McpAzureContainerAppsDemo

mkdir -p src tests
dotnet new web -n McpAzureContainerAppsDemo.Server -o src/McpAzureContainerAppsDemo.Server --framework net10.0
dotnet new console -n McpAzureContainerAppsDemo.Client -o src/McpAzureContainerAppsDemo.Client --framework net10.0
dotnet new xunit -n McpAzureContainerAppsDemo.Server.Tests -o tests/McpAzureContainerAppsDemo.Server.Tests --framework net10.0

dotnet sln add \
  src/McpAzureContainerAppsDemo.Server/McpAzureContainerAppsDemo.Server.csproj \
  src/McpAzureContainerAppsDemo.Client/McpAzureContainerAppsDemo.Client.csproj \
  tests/McpAzureContainerAppsDemo.Server.Tests/McpAzureContainerAppsDemo.Server.Tests.csproj

dotnet add src/McpAzureContainerAppsDemo.Server/McpAzureContainerAppsDemo.Server.csproj package ModelContextProtocol.AspNetCore
dotnet add src/McpAzureContainerAppsDemo.Client/McpAzureContainerAppsDemo.Client.csproj package ModelContextProtocol
dotnet add tests/McpAzureContainerAppsDemo.Server.Tests/McpAzureContainerAppsDemo.Server.Tests.csproj reference src/McpAzureContainerAppsDemo.Server/McpAzureContainerAppsDemo.Server.csproj
```

This repository then adds:

- Central package management in `Directory.Packages.props`.
- Shared build quality settings in `Directory.Build.props`.
- Attribute-discovered MCP tools in `Tools/DemoTools.cs`.
- Testable domain services in `Services/`.
- Optional API-key protection for `/mcp`.
- A multi-stage Dockerfile.

## Run Locally

Start the MCP server:

```bash
dotnet run --project src/McpAzureContainerAppsDemo.Server/McpAzureContainerAppsDemo.Server.csproj --launch-profile http
```

The server listens on:

```text
http://localhost:8080
```

Health check:

```bash
curl http://localhost:8080/health
```

Call the MCP server with the included client:

```bash
dotnet run --project src/McpAzureContainerAppsDemo.Client/McpAzureContainerAppsDemo.Client.csproj -- \
  --url http://localhost:8080/mcp \
  --time-zone Europe/Berlin
```

The client lists the available tools, calls `add`, and calls `server_time`.

## Add Local API-Key Protection

The server only requires an API key when `McpServer__ApiKey` is configured.

```bash
McpServer__ApiKey=local-dev-key \
dotnet run --project src/McpAzureContainerAppsDemo.Server/McpAzureContainerAppsDemo.Server.csproj --launch-profile http
```

Then call it with:

```bash
dotnet run --project src/McpAzureContainerAppsDemo.Client/McpAzureContainerAppsDemo.Client.csproj -- \
  --url http://localhost:8080/mcp \
  --api-key local-dev-key
```

The client sends the key with the `X-Api-Key` header.

## Inspect with MCP Inspector

Start the server, then run:

```bash
npx @modelcontextprotocol/inspector
```

Use the Streamable HTTP transport and connect to:

```text
http://localhost:8080/mcp
```

If API-key protection is enabled, add:

```text
X-Api-Key: local-dev-key
```

## Test and Build

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-restore
```

The sample enables nullable reference types and treats warnings as errors for project code.

## Build and Run the Container Locally

Build from the repository root of this sample:

```bash
docker build -t mcp-aca-demo .
```

Run without API-key protection:

```bash
docker run --rm -p 8080:8080 mcp-aca-demo
```

Run with API-key protection:

```bash
docker run --rm -p 8080:8080 \
  -e McpServer__ApiKey=local-dev-key \
  mcp-aca-demo
```

Then call:

```bash
dotnet run --project src/McpAzureContainerAppsDemo.Client/McpAzureContainerAppsDemo.Client.csproj -- \
  --url http://localhost:8080/mcp \
  --api-key local-dev-key
```

## Deploy to Azure Container Apps

Run these commands from the repository root because `--source .` deploys the current directory and uses the root `Dockerfile`.

Login and configure variables:

```bash
az login

export RESOURCE_GROUP=mcp-aca-demo-rg
export LOCATION=westeurope
export APP_NAME=mcp-aca-demo-$RANDOM
export ENVIRONMENT_NAME=mcp-aca-demo-env
export MCP_API_KEY="$(openssl rand -base64 32)"
```

If this is the first time your subscription uses these services, register the required resource providers:

```bash
az provider register --namespace Microsoft.App
az provider register --namespace Microsoft.OperationalInsights
az provider register --namespace Microsoft.ContainerRegistry
```

Wait for Azure Container Registry registration to finish before deploying:

```bash
az provider show \
  --namespace Microsoft.ContainerRegistry \
  --query registrationState \
  --output tsv
```

Continue when the command prints `Registered`.

Create the resource group:

```bash
az group create \
  --name "$RESOURCE_GROUP" \
  --location "$LOCATION"
```

Deploy the container app from local source:

```bash
az containerapp up \
  --name "$APP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --environment "$ENVIRONMENT_NAME" \
  --source . \
  --ingress external \
  --target-port 8080 \
  --env-vars McpServer__ApiKey="$MCP_API_KEY"
```

Get the public URL:

```bash
export MCP_FQDN="$(az containerapp show \
  --name "$APP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --query properties.configuration.ingress.fqdn \
  --output tsv)"

echo "https://$MCP_FQDN/mcp"
```

Call the deployed MCP server:

```bash
dotnet run --project src/McpAzureContainerAppsDemo.Client/McpAzureContainerAppsDemo.Client.csproj -- \
  --url "https://$MCP_FQDN/mcp" \
  --api-key "$MCP_API_KEY" \
  --time-zone Europe/Berlin
```

## Call with Environment Variables

The client also supports environment variables:

```bash
export MCP_SERVER_URL="https://$MCP_FQDN/mcp"
export MCP_API_KEY="$MCP_API_KEY"
export MCP_TIME_ZONE="Europe/Berlin"

dotnet run --project src/McpAzureContainerAppsDemo.Client/McpAzureContainerAppsDemo.Client.csproj
```

## Cleanup

```bash
az group delete \
  --name "$RESOURCE_GROUP" \
  --yes \
  --no-wait
```

## Security Notes

- This sample uses Streamable HTTP because Azure Container Apps hosts long-running HTTP services, not stdio child processes.
- The server is configured as stateless, which works well for scale-out and container restarts.
- The API-key middleware protects only `/mcp`; `/health` remains unauthenticated for platform health checks.
- The Azure Container Apps walkthrough sets `McpServer__ApiKey` during deployment.
- For production, prefer Microsoft Entra ID or another managed identity-aware authentication layer over a static API key.
- For production, keep secrets in Azure Container Apps secrets or Key Vault. Do not bake secrets into the image.
- Restrict ingress, CORS, logging, and tool capabilities according to the data your real tools can access.
