using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Implementations.Coroutine;
using UniTAS.Plugin.Interfaces.Coroutine;
using UniTAS.Plugin.Interfaces.RuntimeTest;
using UniTAS.Plugin.Models.Coroutine;
using UniTAS.Plugin.Services;
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

    [RuntimeTest]
    public UniTAS.Plugin.Utils.Tuple<bool, IEnumerator<CoroutineWait>> CoroutineTest()
    {
        return new(true, CoroutineTestInner());
    }

    private static IEnumerator<CoroutineWait> CoroutineTestInner()
    {
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
    }

    [RuntimeTest]
    public IEnumerator<CoroutineWait> CoroutineTest2()
    {
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
    }

    [Fact]
    [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
    public void DiscoverTests()
    {
        var kernel = KernelUtils.Init();
        var processor = kernel.GetInstance<IRuntimeTestProcessor>();
        var coroutineRunner = (CoroutineHandler)kernel.GetInstance<ICoroutine>();

        processor.OnDiscoveredTests += count => Assert.Equal(7, count);

        processor.OnTestEnd += results =>
        {
            Assert.Equal(7, results.Count);
            Assert.Equal(4, results.Count(x => x.Passed));
            Assert.Equal(1, results.Count(x => !x.Passed && !x.Skipped));
            Assert.Equal(2, results.Count(x => x.Skipped));
        };
        processor.Test<RuntimeTest>();

        for (var i = 0; i < 2; i++)
        {
            coroutineRunner.PreUpdateUnconditional();
            coroutineRunner.UpdateUnconditional();
        }
    }

    [Fact]
    public void TestFailException()
    {
        var kernel = KernelUtils.Init();
        var processor = kernel.GetInstance<IRuntimeTestProcessor>();
        var coroutineRunner = (CoroutineHandler)kernel.GetInstance<ICoroutine>();

        processor.OnTestEnd += results =>
        {
            var failedResult = results.First(x => !x.Passed);
            Assert.NotNull(failedResult.Exception);
        };

        processor.Test<RuntimeTest>();

        for (var i = 0; i < 2; i++)
        {
            coroutineRunner.PreUpdateUnconditional();
            coroutineRunner.UpdateUnconditional();
        }
    }

    [Fact]
    public void TestPassException()
    {
        var kernel = KernelUtils.Init();
        var processor = kernel.GetInstance<IRuntimeTestProcessor>();
        var coroutineRunner = (CoroutineHandler)kernel.GetInstance<ICoroutine>();

        processor.OnTestEnd += results =>
        {
            var failedResult = results.First(x => x.Passed);
            Assert.Null(failedResult.Exception);
        };

        processor.Test<RuntimeTest>();

        for (var i = 0; i < 2; i++)
        {
            coroutineRunner.PreUpdateUnconditional();
            coroutineRunner.UpdateUnconditional();
        }
    }

    [Fact]
    [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
    public void TestRunEvent()
    {
        var kernel = KernelUtils.Init();
        var processor = kernel.GetInstance<IRuntimeTestProcessor>();
        var coroutineRunner = (CoroutineHandler)kernel.GetInstance<ICoroutine>();

        var testRunCount = 0;
        processor.OnTestRun += _ => testRunCount++;
        processor.OnTestEnd += results =>
        {
            Assert.Equal(7, testRunCount);
            Assert.Equal(testRunCount, results.Count);
        };

        processor.Test<RuntimeTest>();

        for (var i = 0; i < 2; i++)
        {
            coroutineRunner.PreUpdateUnconditional();
            coroutineRunner.UpdateUnconditional();
        }
    }
}