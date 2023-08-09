using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UniTAS.Patcher.Implementations.Coroutine;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Interfaces.RuntimeTest;
using UniTAS.Patcher.Services.RuntimeTest;
using UniTAS.Patcher.Utils;

namespace Patcher.Tests;

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
    public Tuple<bool, IEnumerator<CoroutineWait>> SkipAndCoroutineTest()
    {
        return new(false, null!);
    }

    [RuntimeTest]
    public List<int> WrongReturnType()
    {
        return new();
    }

    [RuntimeTest]
    public Tuple<bool, IEnumerator<CoroutineWait>> CoroutineTest()
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

    [RuntimeTest]
    public IEnumerator<CoroutineWait> CoroutineTestFail()
    {
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
        throw new("Coroutine test failed");
    }

    [Fact]
    [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
    public void DiscoverTests()
    {
        var kernel = KernelUtils.Init();
        var processor = kernel.GetInstance<IRuntimeTestProcessor>();

        processor.OnDiscoveredTests += count => Assert.Equal(8, count);

        processor.OnTestsFinish += results =>
        {
            Assert.Equal(8, results.Results.Count);
            Assert.Equal(4, results.PassedCount);
            Assert.Equal(2, results.FailedCount);
            Assert.Equal(2, results.SkippedCount);
        };
        processor.Test<RuntimeTest>();

        MonoBehaviourController.PausedUpdate = false;
        for (var i = 0; i < 2; i++)
        {
            MonoBehaviourEvents.InvokeUpdate();
            MonoBehaviourEvents.InvokeLateUpdate();
            MonoBehaviourEvents.InvokeFixedUpdate();
        }
    }

    [Fact]
    public void TestFailException()
    {
        var kernel = KernelUtils.Init();
        var processor = kernel.GetInstance<IRuntimeTestProcessor>();

        processor.OnTestsFinish += results =>
        {
            var failedResult = results.Results.First(x => !x.Passed);
            Assert.NotNull(failedResult.Exception);
        };

        processor.Test<RuntimeTest>();

        MonoBehaviourController.PausedUpdate = false;
        for (var i = 0; i < 2; i++)
        {
            MonoBehaviourEvents.InvokeUpdate();
            MonoBehaviourEvents.InvokeLateUpdate();
            MonoBehaviourEvents.InvokeFixedUpdate();
        }
    }

    [Fact]
    public void TestPassException()
    {
        var kernel = KernelUtils.Init();
        var processor = kernel.GetInstance<IRuntimeTestProcessor>();

        processor.OnTestsFinish += results =>
        {
            var failedResult = results.Results.First(x => x.Passed);
            Assert.Null(failedResult.Exception);
        };

        processor.Test<RuntimeTest>();

        MonoBehaviourController.PausedUpdate = false;
        for (var i = 0; i < 2; i++)
        {
            MonoBehaviourEvents.InvokeUpdate();
            MonoBehaviourEvents.InvokeLateUpdate();
            MonoBehaviourEvents.InvokeFixedUpdate();
        }
    }

    [Fact]
    [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
    public void TestRunEvent()
    {
        var kernel = KernelUtils.Init();
        var processor = kernel.GetInstance<IRuntimeTestProcessor>();

        var testRunCount = 0;
        processor.OnTestRun += _ => testRunCount++;
        processor.OnTestsFinish += results =>
        {
            Assert.Equal(8, testRunCount);
            Assert.Equal(testRunCount, results.Results.Count);
        };

        processor.Test<RuntimeTest>();

        MonoBehaviourController.PausedUpdate = false;
        for (var i = 0; i < 2; i++)
        {
            MonoBehaviourEvents.InvokeUpdate();
            MonoBehaviourEvents.InvokeLateUpdate();
            MonoBehaviourEvents.InvokeFixedUpdate();
        }
    }
}