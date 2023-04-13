using HarmonyLib;
using UniTAS.Plugin.Implementations.Coroutine;
using UniTAS.Plugin.Interfaces.Coroutine;
using UniTAS.Plugin.Models.Coroutine;

namespace UniTAS.Plugin.Tests;

public class CoroutineTests
{
    [Fact]
    public void UpdateUnconditional()
    {
        var handler = new CoroutineHandler();
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
        var handler = new CoroutineHandler();
        var updateUnconditionalField = new Traverse(handler).Field("_updateUnconditional")!;

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
}