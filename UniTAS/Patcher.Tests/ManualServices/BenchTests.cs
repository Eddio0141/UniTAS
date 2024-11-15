using System.Linq;
using System.Threading;
using UniTAS.Patcher.ManualServices;

namespace Patcher.Tests.ManualServices;

public class BenchTests
{
    [Fact]
    public void BenchSleep()
    {
        Bench.Reset();
        SleepFunction();
        var stats = Bench.GetStats();
        var entry = stats.Keys.FirstOrDefault(k => k.Section == "SleepFunction");
        var stat = stats[entry];
        Assert.NotNull(stat);
        Assert.True(stat.AverageMs >= 500);
    }

    [Fact]
    public void BenchSleepDiscard()
    {
        Bench.Reset();
        SleepFunctionUsingDiscard();
        var stats = Bench.GetStats();
        var entry = stats.Keys.FirstOrDefault(k => k.Section == "SleepFunctionUsingDiscard");
        var stat = stats[entry];
        Assert.NotNull(stat);
        Assert.True(stat.AverageMs >= 500);
    }

    [Fact]
    public void BenchSleepMultiple()
    {
        Bench.Reset();
        for (var i = 0; i < 6; i++)
        {
            SleepFunction();
        }

        var stats = Bench.GetStats();
        var entry = stats.Keys.FirstOrDefault(k => k.Section == "SleepFunction");
        var stat = stats[entry];
        Assert.NotNull(stat);
        Assert.True(stat.AverageMs >= 500);
        Assert.Equal(6, stat.SampleCount);
    }

    private static void SleepFunction()
    {
        var m = Bench.Measure();
        Thread.Sleep(500);
        m.Dispose();
    }

    private static void SleepFunctionUsingDiscard()
    {
        using var _ = Bench.Measure();
        Thread.Sleep(500);
    }
}