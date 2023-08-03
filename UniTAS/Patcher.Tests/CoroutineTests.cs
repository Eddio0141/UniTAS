using System.Collections.Generic;
using UniTAS.Patcher.Implementations.Coroutine;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Models.Coroutine;
using UniTAS.Patcher.Services;

namespace Patcher.Tests;

public class CoroutineTests
{
    [Fact]
    public void UpdateUnconditional()
    {
        var kernel = KernelUtils.Init();
        var handler = kernel.GetInstance<CoroutineHandler>();
        var status = handler.Start(UpdateUnconditionalCoroutine());

        Assert.True(status.IsRunning);

        handler.UpdateUnconditional();
        Assert.True(status.IsRunning);

        handler.UpdateUnconditional();
        Assert.True(status.IsRunning);

        handler.UpdateUnconditional();
        Assert.False(status.IsRunning);
    }

    [Fact]
    public void EmptyCoroutineTest()
    {
        var kernel = KernelUtils.Init();
        var handler = kernel.GetInstance<CoroutineHandler>();

        var status = handler.Start(EmptyCoroutine());
        Assert.False(status.IsRunning);
    }

    private static IEnumerator<CoroutineWait> UpdateUnconditionalCoroutine()
    {
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
    }

    private static IEnumerator<CoroutineWait> EmptyCoroutine()
    {
        yield break;
    }

    [Fact]
    public void SyncFixedUpdateCoroutineTest()
    {
        var kernel = KernelUtils.Init();
        var handler = kernel.GetInstance<CoroutineHandler>();
        var syncFixedUpdateCycle = kernel.GetInstance<KernelUtils.SyncFixedUpdateCycleDummy>();

        var status = handler.Start(SyncFixedUpdateCoroutineTestCoroutine());
        Assert.True(status.IsRunning);
        syncFixedUpdateCycle.ForceLastCallback();
        Assert.False(status.IsRunning);
    }

    private static IEnumerator<CoroutineWait> SyncFixedUpdateCoroutineTestCoroutine()
    {
        yield return new WaitForOnSync();
    }
}