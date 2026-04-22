using Xunit;
using MiniTransportTycoon.Core.Map;

public class ForestTests
{
    [Fact]
    public void ForestGrowAfter5Seconds()
    {
        var forest = new Forest();
        forest.Tick(5f);
        Assert.Equal(2, forest.TreeCount);
    }

    [Fact]
    public void ForestNotExceedMax()
    {
        var forest = new Forest();
        forest.Tick(100f);
        Assert.Equal(4, forest.TreeCount);
    }

    [Fact]
    public void ForestNotGrowBeforeThreshold()
    {
        var forest = new Forest();
        forest.Tick(4.9f);
        Assert.Equal(1, forest.TreeCount);
    }

    [Fact]
    public void ForestNotSpreadWhenTooSmall()
    {
        var forest = new Forest();
        bool result = forest.UpdateSpread(10f);
        Assert.False(result);
    }

    [Fact]
    public void ForestSpreadWhenLargeEnough()
    {
        var forest = new Forest();
        forest.Tick(15f);
        bool result = forest.UpdateSpread(5f);
        Assert.True(result);
    }
    [Fact]
    public void ForestGrowAndSpread()
    {
        var forest = new Forest();
        forest.Tick(15f);
        Assert.Equal(4, forest.TreeCount);

        bool spread = forest.UpdateSpread(5f);
        Assert.True(spread);
    }
}