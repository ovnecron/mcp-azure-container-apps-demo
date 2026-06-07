namespace McpAzureContainerAppsDemo.Server.Services;

public sealed class CalculatorService
{
    public int Add(int left, int right) => checked(left + right);
}
