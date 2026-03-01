using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.SceneManagement;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "CheckNamespace")]
public class InstantiateAsync__2022_3__6000_0_44f1 : MonoBehaviour
{
    [TestInjectPrefab] public GameObject prefab;
    [TestInjectScene] public string emptyScene;

    [Test]
    public void ForceSameFrameLoadWithActivation()
    {
        // interesting enough, this makes it load on the same frame
        var completedCalled = false;
        var frameCount = Time.frameCount;
        var initOp = InstantiateAsync(prefab);
        initOp.completed += _ =>
        {
            Assert.Equal(0, Time.frameCount - frameCount);
            completedCalled = true;
        };
        initOp.allowSceneActivation = false;
        Assert.False(initOp.isDone);
        Assert.True(initOp.IsWaitingForSceneActivation());
        initOp.allowSceneActivation = true;
        Assert.True(initOp.isDone);
        Assert.NotNull(initOp.Result);
        Assert.True(completedCalled);
    }

    [Test]
    public IEnumerator<TestYield> ForceSameFrameLoadWithActivationMultiple()
    {
        // see if multiple load, with first op force loaded will chain multiple
        // ---
        // result: only ones that we explicitly pick to force load is loaded instantly
        var op = InstantiateAsync(prefab);
        op.allowSceneActivation = false;
        var op2 = InstantiateAsync(prefab);
        op2.allowSceneActivation = false;
        var op3 = InstantiateAsync(prefab);
        op3.allowSceneActivation = false;
        Assert.False(op.isDone);
        Assert.False(op2.isDone);
        Assert.False(op3.isDone);

        yield return new UnityYield(null);
        op.allowSceneActivation = true;

        Assert.True(op.isDone);
        Assert.False(op2.isDone);
        Assert.False(op3.isDone);

        op3.allowSceneActivation = true;

        Assert.True(op.isDone);
        Assert.False(op2.isDone);
        Assert.True(op3.isDone);

        op2.allowSceneActivation = true;

        Assert.True(op.isDone);
        Assert.True(op2.isDone);
        Assert.True(op3.isDone);
    }

    [Test]
    public IEnumerator<TestYield> StallLoadWithYield()
    {
        // real stall test
        var initOp = InstantiateAsync(prefab);
        Assert.False(initOp.IsWaitingForSceneActivation());
        initOp.allowSceneActivation = false;
#if !UNITY_EDITOR
        Assert.True(initOp.IsWaitingForSceneActivation());
#endif
        Assert.False(initOp.isDone);
        Assert.Null(initOp.Result);
        yield return new UnityYield(null);
        Assert.Null(initOp.Result);
        yield return new UnityYield(null);
        Assert.Null(initOp.Result);
        yield return new UnityYield(null);
        Assert.Null(initOp.Result);
        yield return new UnityYield(null);
        Assert.Null(initOp.Result);
        initOp.allowSceneActivation = true;
        // again, result is ready immediately unlike scene loading
        Assert.True(initOp.isDone);
        Assert.False(initOp.IsWaitingForSceneActivation());
    }

    [Test]
    public IEnumerator<TestYield> ForceObjectInitBlocking()
    {
        // try force object init immediate
        var frameCount = Time.frameCount;
        var initOp = InstantiateAsync(prefab);
        var initOp2 = InstantiateAsync(prefab);
        var completedCalled = false;
        var completedCalledNonBlocking = false;
        initOp.completed += _ =>
        {
            Assert.Equal(0, Time.frameCount - frameCount);
            completedCalled = true;
        };
        initOp2.completed += _ => completedCalledNonBlocking = true;
        initOp.WaitForCompletion();
        Assert.True(completedCalled);
        Assert.True(initOp.isDone);
        Assert.False(initOp.IsWaitingForSceneActivation());
        Assert.False(initOp2.isDone);
        Assert.False(completedCalledNonBlocking);
        Assert.False(initOp2.IsWaitingForSceneActivation());
        yield return new UnityYield(initOp2);
        yield return new UnityYield(null);
        Assert.True(completedCalledNonBlocking);
    }

    [Test]
    public IEnumerator<TestYield> InstantiateOrder()
    {
        // normal, 1f async object init
        var initOp = InstantiateAsync(prefab);
        var initOp2 = InstantiateAsync(prefab);
        Assert.Null(initOp.Result);
        Assert.Null(initOp2.Result);
        Assert.False(initOp.IsWaitingForSceneActivation());
        Assert.False(initOp2.IsWaitingForSceneActivation());
        var frameCount = Time.frameCount;
        var completeCalled = false;
        initOp.completed += _ =>
        {
            Assert.Equal(1, Time.frameCount - frameCount);
            completeCalled = true;
        };
        initOp2.completed += _ => { Assert.Equal(1, Time.frameCount - frameCount); };
        Assert.False(initOp.isDone);
        Assert.False(initOp2.isDone);
        Assert.False(completeCalled);
        yield return new UnityYield(new WaitForEndOfFrame());
        Assert.False(initOp.isDone);
        Assert.False(completeCalled);
        Assert.Null(initOp.Result);
        Assert.Null(initOp2.Result);
        yield return new UnityYield(null);
        Assert.True(initOp.isDone);
        Assert.True(initOp2.isDone);
        Assert.False(initOp.IsWaitingForSceneActivation());
        Assert.False(initOp2.IsWaitingForSceneActivation());
    }

    [Test]
    public IEnumerator<TestYield> InstantiateOrderBlocking()
    {
        // check if second operation gets blocked by first operation being stalled
        // ---
        // result: second operation however gets processed before first, as they aren't in a queue
        var initOp = InstantiateAsync(prefab);
        initOp.allowSceneActivation = false;
        var completeCalled = false;
        initOp.completed += _ => completeCalled = true;
        var initOp2 = InstantiateAsync(prefab);
        Assert.False(initOp.isDone);
        Assert.False(initOp2.isDone);

        yield return new UnityYield(new WaitForEndOfFrame());

        Assert.False(initOp.isDone);
        Assert.False(initOp2.isDone);
        yield return new UnityYield(null);

        Assert.False(initOp.isDone);
        Assert.True(initOp2.isDone);
        yield return new UnityYield(null);

        initOp.allowSceneActivation = true;

        Assert.True(initOp.isDone);
        Assert.True(initOp2.isDone);
        Assert.True(completeCalled);
    }

    [Test]
    public IEnumerator<TestYield> SceneLoadStallInteration()
    {
        // check if scene load stall blocks instantiate async operations
        // ---
        // result: instantiate operation isn't blocked by scene load operation
        var loadEmptyScene = SceneManager.LoadSceneAsync(emptyScene, LoadSceneMode.Additive);
        loadEmptyScene.allowSceneActivation = false;
        var initOp = InstantiateAsync(prefab);

        yield return new UnityYield(null);
        Assert.True(initOp.isDone);
    }

    [Test]
    public IEnumerator<TestYield> LoadOrderSceneAsyncLoad()
    {
        // check scene load order when loaded along instantiation
        // ---
        // result: instantiate > scene load
        var sceneOp = SceneManager.LoadSceneAsync(emptyScene, LoadSceneMode.Additive);
        string first = null;
        sceneOp.completed += _ => first ??= "sceneOp";
        Assert.False(sceneOp.isDone);

        yield return new UnityYield(null);
        var op = InstantiateAsync(prefab);
        op.completed += _ => first ??= "op";

        Assert.False(op.isDone);
        Assert.False(sceneOp.isDone);

        yield return new UnityYield(null);

        Assert.True(op.isDone);
        Assert.True(sceneOp.isDone);
        Assert.Equal(first, "op");

        // try again, but instantiate after scene load. needs stalling since scene load takes +1 frame
        op = InstantiateAsync(prefab);
        op.completed += _ => first ??= "op";
        op.allowSceneActivation = false;
        sceneOp = SceneManager.LoadSceneAsync(emptyScene, LoadSceneMode.Additive);
        sceneOp.completed += _ => first ??= "sceneOp";
        first = null;

        yield return new UnityYield(null);

        Assert.False(op.isDone);
        Assert.False(sceneOp.isDone);

        yield return new UnityYield(null);
        op.allowSceneActivation = true;

        Assert.True(op.isDone);
        Assert.True(sceneOp.isDone);
        Assert.Equal(first, "sceneOp");
    }

    [Test]
    public IEnumerator<TestYield> SceneSyncLoadDuringInstantiate()
    {
        // check if non-async load scene forces init
        // ---
        // result: it doesn't influence instantiate stall
        var op = InstantiateAsync(prefab);
        op.allowSceneActivation = false;
        Assert.False(op.isDone);

        SceneManager.LoadScene(emptyScene);

        Assert.False(op.isDone);

        yield return new UnityYield(null);
        Assert.Equal(SceneManager.GetActiveScene().path, emptyScene, "sanity check fail");

        Assert.False(op.isDone);

        op.allowSceneActivation = true;
        Assert.True(op.isDone, "sanity check fail");
    }

    [Test]
    public IEnumerator<TestYield> SceneAsyncLoadInitBlocking()
    {
        // check if forced object init affects scene load operation
        // ---
        // result: not linked
        var op = InstantiateAsync(prefab);
        var sceneOp = SceneManager.LoadSceneAsync(emptyScene, LoadSceneMode.Additive);

        op.WaitForCompletion();
        Assert.True(op.isDone);
        Assert.False(sceneOp.isDone);

        yield return new UnityYield(null);
        Assert.False(sceneOp.isDone);

        yield return new UnityYield(null);
        Assert.True(sceneOp.isDone);
    }

    [Test]
    public IEnumerator<TestYield> InstantiateDuringScene1FDelay()
    {
        SceneManager.LoadSceneAsync(emptyScene);
        var initOp = InstantiateAsync(prefab);
        yield return new UnityYield(null);
        // now in 1f delay
        Assert.True(initOp.isDone);
    }
}
