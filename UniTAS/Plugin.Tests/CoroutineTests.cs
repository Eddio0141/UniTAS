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
        var updateUnconditionalField = new Traverse(handler).Field("_updateUnconditional")!;

        handler.Start(UpdateUnconditionalCoroutine());
        Assert.Single(updateUnconditionalField.GetValue<Queue<IEnumerator<CoroutineWait>>>());
        handler.UpdateUnconditional();
        Assert.Single(updateUnconditionalField.GetValue<Queue<IEnumerator<CoroutineWait>>>());
        handler.UpdateUnconditional();
        Assert.Single(updateUnconditionalField.GetValue<Queue<IEnumerator<CoroutineWait>>>());
        handler.UpdateUnconditional();
        Assert.Empty(updateUnconditionalField.GetValue<Queue<IEnumerator<CoroutineWait>>>());
    }

    [Fact]
    public void EmptyCoroutineTest()
    {
        var handler = new CoroutineHandler();
        var updateUnconditionalField = new Traverse(handler).Field("_updateUnconditional")!;

        handler.Start(EmptyCoroutine());
        Assert.Empty(updateUnconditionalField.GetValue<Queue<IEnumerator<CoroutineWait>>>());
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