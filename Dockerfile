FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./
COPY src/McpAzureContainerAppsDemo.Server/McpAzureContainerAppsDemo.Server.csproj src/McpAzureContainerAppsDemo.Server/
RUN dotnet restore src/McpAzureContainerAppsDemo.Server/McpAzureContainerAppsDemo.Server.csproj

COPY src/McpAzureContainerAppsDemo.Server/ src/McpAzureContainerAppsDemo.Server/
RUN dotnet publish src/McpAzureContainerAppsDemo.Server/McpAzureContainerAppsDemo.Server.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "McpAzureContainerAppsDemo.Server.dll"]
