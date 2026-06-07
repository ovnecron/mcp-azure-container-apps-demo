using McpAzureContainerAppsDemo.Server.Services;

namespace McpAzureContainerAppsDemo.Server.Tests;

public sealed class CalculatorServiceTests
{
    [Fact]
    public void Add_ReturnsSum()
    {
        var calculator = new CalculatorService();

        int result = calculator.Add(40, 2);

        Assert.Equal(42, result);
    }

    [Fact]
    public void Add_ThrowsOnOverflow()
    {
        var calculator = new CalculatorService();

        Assert.Throws<OverflowException>(() => calculator.Add(int.MaxValue, 1));
    }
}
