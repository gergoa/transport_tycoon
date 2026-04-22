using Xunit;
using MiniTransportTycoon.Game.Time;

public class TimeManagerTests
{
    [Fact]
    public void DefaultSpeedNormal()
    {
        var tm = new TimeManager();
        Assert.Equal(1f, tm.Tick(1f));
    }

    [Fact]
    public void PausedReturnZero()
    {
        var tm = new TimeManager();
        tm.SetSpeed(TimeSpeed.Paused);
        Assert.Equal(0f, tm.Tick(1f));
    }

    [Fact]
    public void FastDoubleTime()
    {
        var tm = new TimeManager();
        tm.SetSpeed(TimeSpeed.Fast);

        Assert.Equal(4f, tm.Tick(2f));
    }

    [Fact]
    public void VeryFastQuadrupleTime()
    {
        var tm = new TimeManager();
        tm.SetSpeed(TimeSpeed.VeryFast);

        Assert.Equal(8f, tm.Tick(2f));
    }

    [Fact]
    public void LargeDeltaTimeScaleCorrectly()
    {
        var tm = new TimeManager();
        tm.SetSpeed(TimeSpeed.Fast);

        Assert.Equal(2000f, tm.Tick(1000f));
    }
}  