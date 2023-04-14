using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces.Coroutine;
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

    [RuntimeTest]
    public UniTAS.Plugin.Utils.Tuple<bool, IEnumerator<CoroutineWait>> SkipAndCoroutineTest()
    {
        return new(false, null!);
    }

    [RuntimeTest]
    public List<int> WrongReturnType()
    {
        return new();
    }

    [Fact]
    public void DiscoverTests()
    {
        var kernel = KernelUtils.Init();
        var processor = kernel.GetInstance<IRuntimeTestProcessor>();
        processor.OnTestEnd += results =>
        {
            processor.OnDiscoveredTests += count => Assert.Equal(2, count);

            Assert.Equal(5, results.Count);
            Assert.Equal(2, results.Count(x => x.Passed));
            Assert.Equal(1, results.Count(x => !x.Passed && !x.Skipped));
            Assert.Equal(2, results.Count(x => x.Skipped));
        };
        processor.Test<RuntimeTest>();
    }

    [Fact]
    public void TestFailException()
    {
        var kernel = KernelUtils.Init();
        var processor = kernel.GetInstance<IRuntimeTestProcessor>();

        processor.OnTestEnd += results =>
        {
            var failedResult = results.First(x => !x.Passed);
            Assert.NotNull(failedResult.Exception);
        };

        processor.Test<RuntimeTest>();
    }

    [Fact]
    public void TestPassException()
    {
        var kernel = KernelUtils.Init();
        var processor = kernel.GetInstance<IRuntimeTestProcessor>();

        processor.OnTestEnd += results =>
        {
            var failedResult = results.First(x => x.Passed);
            Assert.Null(failedResult.Exception);
        };

        processor.Test<RuntimeTest>();
    }

    [Fact]
    public void TestRunEvent()
    {
        var kernel = KernelUtils.Init();
        var processor = kernel.GetInstance<IRuntimeTestProcessor>();

        var testRunCount = 0;
        processor.OnTestRun += _ => testRunCount++;
        processor.OnTestEnd += results =>
        {
            Assert.Equal(5, testRunCount);
            Assert.Equal(testRunCount, results.Count);
        };

        processor.Test<RuntimeTest>();
    }
}