using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UniTAS.Patcher.Services.RuntimeTest;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.SingletonBindings.GameExecutionControllers;

namespace Patcher.Tests.RuntimeTests;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class RuntimeUnitTest
{
    [Fact]
    [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
    public void DiscoverTests()
    {
        var kernel = KernelUtils.Init();
        var processor = kernel.GetInstance<IRuntimeTestProcessor>();
        var monoBehEventInvoker = kernel.GetInstance<IMonoBehEventInvoker>();

        processor.OnDiscoveredTests += count => Assert.Equal(RuntimeTestsUtils.TotalCount, count);

        processor.OnTestsFinish += results =>
        {
            Assert.Equal(RuntimeTestsUtils.TotalCount, results.Results.Count);
            Assert.Equal(RuntimeTestsUtils.PassCount, results.PassedCount);
            Assert.Equal(RuntimeTestsUtils.FailCount, results.FailedCount);
            Assert.Equal(RuntimeTestsUtils.SkipNormalTestCount + RuntimeTestsUtils.SkippedCoroutineTestCount,
                results.SkippedCount);
        };
        processor.Test<RuntimeTests>();

        MonoBehaviourController.PausedUpdate = false;
        for (var i = 0; i < 2; i++)
        {
            monoBehEventInvoker.InvokeUpdate();
            monoBehEventInvoker.InvokeLateUpdate();
            monoBehEventInvoker.InvokeFixedUpdate();
        }
    }

    [Fact]
    public void TestFailException()
    {
        var kernel = KernelUtils.Init();
        var processor = kernel.GetInstance<IRuntimeTestProcessor>();
        var monoBehEventInvoker = kernel.GetInstance<IMonoBehEventInvoker>();

        processor.OnTestsFinish += results =>
        {
            var failedResult = results.Results.First(x => !x.Passed);
            Assert.NotNull(failedResult.Exception);
        };

        processor.Test<RuntimeTests>();

        MonoBehaviourController.PausedUpdate = false;
        for (var i = 0; i < 2; i++)
        {
            monoBehEventInvoker.InvokeUpdate();
            monoBehEventInvoker.InvokeLateUpdate();
            monoBehEventInvoker.InvokeFixedUpdate();
        }
    }

    [Fact]
    public void TestPassException()
    {
        var kernel = KernelUtils.Init();
        var processor = kernel.GetInstance<IRuntimeTestProcessor>();
        var monoBehEventInvoker = kernel.GetInstance<IMonoBehEventInvoker>();

        processor.OnTestsFinish += results =>
        {
            var failedResult = results.Results.First(x => x.Passed);
            Assert.Null(failedResult.Exception);
        };

        processor.Test<RuntimeTests>();

        MonoBehaviourController.PausedUpdate = false;
        for (var i = 0; i < 2; i++)
        {
            monoBehEventInvoker.InvokeUpdate();
            monoBehEventInvoker.InvokeLateUpdate();
            monoBehEventInvoker.InvokeFixedUpdate();
        }
    }

    [Fact]
    [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
    public void TestRunEvent()
    {
        var kernel = KernelUtils.Init();
        var processor = kernel.GetInstance<IRuntimeTestProcessor>();
        var monoBehEventInvoker = kernel.GetInstance<IMonoBehEventInvoker>();

        var testRunCount = 0;
        processor.OnTestRun += _ => testRunCount++;
        processor.OnTestsFinish += results =>
        {
            Assert.Equal(RuntimeTestsUtils.TotalCount - RuntimeTestsUtils.SkippedCoroutineTestCount, testRunCount);
            Assert.Equal(RuntimeTestsUtils.TotalCount, results.Results.Count);
        };

        processor.Test<RuntimeTests>();

        MonoBehaviourController.PausedUpdate = false;
        for (var i = 0; i < 2; i++)
        {
            monoBehEventInvoker.InvokeUpdate();
            monoBehEventInvoker.InvokeLateUpdate();
            monoBehEventInvoker.InvokeFixedUpdate();
        }
    }

    [Fact]
    public void CheckCoroutineTestEnd()
    {
        var kernel = KernelUtils.Init();
        var processor = kernel.GetInstance<IRuntimeTestProcessor>();
        var monoBehEventInvoker = kernel.GetInstance<IMonoBehEventInvoker>();

        var coroutineTestRunCount = 0;
        var coroutineTestEndCount = 0;
        var normalTestRunCount = 0;
        var normalTestEndCount = 0;
        processor.OnTestRun += name =>
        {
            if (RuntimeTestsUtils.CoroutineTests.Contains(name))
            {
                coroutineTestRunCount++;
            }
            else
            {
                normalTestRunCount++;
            }
        };
        processor.OnTestEnd += result =>
        {
            if (RuntimeTestsUtils.CoroutineTests.Any(x => result.TestName.EndsWith(x)))
            {
                coroutineTestEndCount++;
            }
            else
            {
                normalTestEndCount++;
            }
        };

        processor.Test<RuntimeTests>();

        // normal tests should be all finished (and the skipped coroutine test)
        Assert.Equal(RuntimeTestsUtils.NormalTestCount + RuntimeTestsUtils.SkippedCoroutineTestCount,
            normalTestRunCount);
        Assert.Equal(normalTestRunCount, normalTestEndCount + RuntimeTestsUtils.SkippedCoroutineTestCount);

        // but not coroutines
        Assert.Equal(1, coroutineTestEndCount);
        Assert.Equal(0, coroutineTestRunCount);

        MonoBehaviourController.PausedUpdate = false;
        for (var i = 0; i < 2; i++)
        {
            monoBehEventInvoker.InvokeUpdate();
            monoBehEventInvoker.InvokeLateUpdate();
            monoBehEventInvoker.InvokeFixedUpdate();
        }

        // and now coroutines should be finished too
        Assert.Equal(
            RuntimeTestsUtils.CoroutineTests.Length -
            RuntimeTestsUtils.SkippedCoroutineTestCount -
            RuntimeTestsUtils.FailedCoroutineTestCount,
            coroutineTestEndCount);
    }
}