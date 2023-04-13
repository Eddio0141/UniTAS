using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces.RuntimeTest;
using UniTAS.Plugin.Services.RuntimeTest;

namespace UniTAS.Plugin.Tests;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class RuntimeTest
{
    [RuntimeTest]
    private void RuntimeTestMethod()
    {
    }

    [RuntimeTest]
    private void RuntimeTestFail()
    {
        throw new("Runtime test failed");
    }

    [RuntimeTest]
    private bool SkipTest()
    {
        return false;
    }

    [Fact]
    public void DiscoverTests()
    {
        var kernel = KernelUtils.Init();
        var processor = kernel.GetInstance<IRuntimeTestProcessor>();
        var results = processor.Test<RuntimeTest>();

        processor.OnDiscoveredTests += count => Assert.Equal(2, count);

        Assert.Equal(3, results.Count);
        Assert.Equal(1, results.Count(x => x.Passed));
        Assert.Equal(2, results.Count(x => !x.Passed));
        Assert.Equal(1, results.Count(x => x.Skipped));
    }

    [Fact]
    public void TestFailException()
    {
        var kernel = KernelUtils.Init();
        var results = kernel.GetInstance<IRuntimeTestProcessor>().Test<RuntimeTest>();

        var failedResult = results.First(x => !x.Passed);
        Assert.NotNull(failedResult.Exception);
    }

    [Fact]
    public void TestPassException()
    {
        var kernel = KernelUtils.Init();
        var results = kernel.GetInstance<IRuntimeTestProcessor>().Test<RuntimeTest>();

        var failedResult = results.First(x => x.Passed);
        Assert.Null(failedResult.Exception);
    }

    [Fact]
    public void TestRunEvent()
    {
        var kernel = KernelUtils.Init();
        var processor = kernel.GetInstance<IRuntimeTestProcessor>();

        var testRunCount = 0;
        processor.OnTestRun += _ => testRunCount++;

        var results = processor.Test<RuntimeTest>();

        Assert.Equal(3, testRunCount);
        Assert.Equal(3, results.Count);
    }
}