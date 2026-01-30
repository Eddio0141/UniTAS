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
        initOp2.completed += _ => { completedCalledNonBlocking = true; };
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
    public IEnumerator<TestYield> TripleLoadTogether()
    {
        // normal, 1f async object init
        var initOp = InstantiateAsync(prefab);
        var initOp2 = InstantiateAsync(prefab);
        var initOp3 = InstantiateAsync(prefab);
        Assert.Null(initOp.Result);
        Assert.Null(initOp2.Result);
        Assert.Null(initOp3.Result);
        Assert.False(initOp.IsWaitingForSceneActivation());
        Assert.False(initOp2.IsWaitingForSceneActivation());
        Assert.False(initOp3.IsWaitingForSceneActivation());
        var frameCount = Time.frameCount;
        var completeCalled = false;
        initOp.completed += _ =>
        {
            Assert.Equal(1, Time.frameCount - frameCount);
            completeCalled = true;
        };
        initOp2.completed += _ => { Assert.Equal(1, Time.frameCount - frameCount); };
        initOp3.completed += _ => { Assert.Equal(1, Time.frameCount - frameCount); };
        Assert.False(initOp.isDone);
        Assert.False(initOp2.isDone);
        Assert.False(initOp3.isDone);
        Assert.False(completeCalled);
        yield return new UnityYield(new WaitForEndOfFrame());
        Assert.False(initOp.isDone);
        Assert.False(completeCalled);
        Assert.Null(initOp.Result);
        Assert.Null(initOp2.Result);
        Assert.Null(initOp3.Result);
        yield return new UnityYield(null);
        Assert.True(initOp.isDone);
        Assert.True(initOp2.isDone);
        Assert.True(initOp3.isDone);
        Assert.False(initOp.IsWaitingForSceneActivation());
        Assert.False(initOp2.IsWaitingForSceneActivation());
        Assert.False(initOp3.IsWaitingForSceneActivation());
    }

    [Test]
    public IEnumerator<TestYield> InstantiateDuringSceneLoadStall()
    {
        var initOp = InstantiateAsync(prefab);
        var loadEmptyScene = SceneManager.LoadSceneAsync(emptyScene, LoadSceneMode.Additive)!;
        loadEmptyScene.allowSceneActivation = false;
        yield return new UnityYield(null);
        Assert.True(initOp.isDone);

        // what about loading with blocking thread? would that force the scene load or not
        initOp = InstantiateAsync(prefab);
        initOp.WaitForCompletion();
        Assert.True(initOp.isDone);
        yield return new UnityYield(null);
        Assert.False(loadEmptyScene.isDone);
    }

    [Test]
    public IEnumerator<TestYield> SceneSyncLoadDuringInstantiate()
    {
        // now try sync loading scene, see if it forces init
        var asyncInit = InstantiateAsync(prefab);
        asyncInit.allowSceneActivation = false;
        Assert.Null(asyncInit.Result);

        yield return new SceneSwitchYield(emptyScene);

        Assert.False(asyncInit.allowSceneActivation);
        Assert.False(asyncInit.isDone);
        asyncInit.allowSceneActivation = true;
        Assert.True(asyncInit.isDone);
        Assert.NotNull(asyncInit.Result[0]);
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
