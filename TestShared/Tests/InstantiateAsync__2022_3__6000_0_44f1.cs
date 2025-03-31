using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "CheckNamespace")]
public class InstantiateAsync__2022_3__6000_0_44f1 : MonoBehaviour
{
    public ITestAsset emptyObjectAsset => new GameObjectAsset();
    [TestInjectPrefab(nameof(emptyObjectAsset))] public GameObject emptyPrefab;
    [TestInjectScene] public string emptyScene;

    [Test]
    public void ForceSameFrameLoadWithActivation()
    {
        // interesting enough, this makes it load on the same frame
        var completedCalled = false;
        var frameCount = Time.frameCount;
        var initOp = InstantiateAsync(emptyPrefab);
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
        var op = InstantiateAsync(emptyPrefab);
        op.allowSceneActivation = false;
        var op2 = InstantiateAsync(emptyPrefab);
        op2.allowSceneActivation = false;
        var op3 = InstantiateAsync(emptyPrefab);
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
        var initOp = InstantiateAsync(emptyPrefab);
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
        var initOp = InstantiateAsync(emptyPrefab);
        var initOp2 = InstantiateAsync(emptyPrefab);
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
        var initOp = InstantiateAsync(emptyPrefab);
        var initOp2 = InstantiateAsync(emptyPrefab);
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
        var initOp = InstantiateAsync(emptyPrefab);
        initOp.allowSceneActivation = false;
        var completeCalled = false;
        initOp.completed += _ => completeCalled = true;
        var initOp2 = InstantiateAsync(emptyPrefab);
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
        var initOp = InstantiateAsync(emptyPrefab);

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
        var op = InstantiateAsync(emptyPrefab);
        op.completed += _ => first ??= "op";

        Assert.False(op.isDone);
        Assert.False(sceneOp.isDone);

        yield return new UnityYield(null);

        Assert.True(op.isDone);
        Assert.True(sceneOp.isDone);
        Assert.Equal(first, "op");

        // try again, but instantiate after scene load. needs stalling since scene load takes +1 frame
        op = InstantiateAsync(emptyPrefab);
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
        var op = InstantiateAsync(emptyPrefab);
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
        var op = InstantiateAsync(emptyPrefab);
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
        var initOp = InstantiateAsync(emptyPrefab);
        yield return new UnityYield(null);
        // now in 1f delay
        Assert.True(initOp.isDone);
    }

    [Test]
    public IEnumerator<TestYield> PosAndRotEmptyExplicit()
    {
        var positions = new ReadOnlySpan<Vector3>();
        var rotations = new ReadOnlySpan<Quaternion>();
        var op = InstantiateAsync(emptyPrefab, 3, positions, rotations);
        yield return new UnityYield(op);

        Assert.Equal(op.Result.Length, 3);

        foreach (var obj in op.Result)
        {
            var t = obj.transform;
            Assert.Equal(t.position, Vector3.zero);
            Assert.Equal(t.rotation, Quaternion.identity);
        }
    }

    [Test]
    public IEnumerator<TestYield> PosAndRotPartEmpty()
    {
        var positions = new ReadOnlySpan<Vector3>(new[] { new Vector3(1, 1, 1), new Vector3(2, 2, 2), new Vector3(3, 3, 3) });
        var rotations = new ReadOnlySpan<Quaternion>();
        var op = InstantiateAsync(emptyPrefab, 3, positions, rotations);
        yield return new UnityYield(op);

        Assert.Equal(op.Result.Length, 3);
        var transforms = op.Result.Select(x => x.transform).ToArray();

        foreach (var t in transforms)
        {
            Assert.Equal(t.rotation, Quaternion.identity);
        }

        Assert.Equal(transforms[0].position, new Vector3(1, 1, 1));
        Assert.Equal(transforms[1].position, new Vector3(2, 2, 2));
        Assert.Equal(transforms[2].position, new Vector3(3, 3, 3));
    }

    [Test]
    public IEnumerator<TestYield> PosAndRotEqual()
    {
        var positions = new ReadOnlySpan<Vector3>(new[] { new Vector3(1, 1, 1), new Vector3(2, 2, 2), new Vector3(3, 3, 3) });
        var rotations = new ReadOnlySpan<Quaternion>(new[] { Quaternion.Euler(new Vector3(1, 1, 1)), Quaternion.Euler(new Vector3(2, 2, 2)), Quaternion.Euler(new Vector3(3, 3, 3)) });
        var op = InstantiateAsync(emptyPrefab, 3, positions, rotations);
        yield return new UnityYield(op);

        Assert.Equal(op.Result.Length, 3);
        var transforms = op.Result.Select(x => x.transform).ToArray();

        Assert.Equal(transforms[0].rotation.eulerAngles, new Vector3(1, 1, 1), 0.000001f);
        Assert.Equal(transforms[1].rotation.eulerAngles, new Vector3(2, 2, 2), 0.000001f);
        Assert.Equal(transforms[2].rotation.eulerAngles, new Vector3(3, 3, 3), 0.000001f);

        Assert.Equal(transforms[0].position, new Vector3(1, 1, 1));
        Assert.Equal(transforms[1].position, new Vector3(2, 2, 2));
        Assert.Equal(transforms[2].position, new Vector3(3, 3, 3));
    }

    [Test]
    public IEnumerator<TestYield> PosAndRotUnbalanced()
    {
        var positions = new ReadOnlySpan<Vector3>(new[] { new Vector3(1, 1, 1), new Vector3(2, 2, 2), new Vector3(3, 3, 3) });
        var rotations = new ReadOnlySpan<Quaternion>(new[] { Quaternion.Euler(new Vector3(1, 1, 1)), Quaternion.Euler(new Vector3(2, 2, 2)) });
        var op = InstantiateAsync(emptyPrefab, 4, positions, rotations);
        yield return new UnityYield(op);

        Assert.Equal(op.Result.Length, 4);
        var transforms = op.Result.Select(x => x.transform).ToArray();

        Assert.Equal(transforms[0].rotation.eulerAngles, new Vector3(1, 1, 1), 0.000001f);
        Assert.Equal(transforms[1].rotation.eulerAngles, new Vector3(2, 2, 2), 0.000001f);
        Assert.Equal(transforms[2].rotation.eulerAngles, new Vector3(1, 1, 1), 0.000001f);
        Assert.Equal(transforms[3].rotation.eulerAngles, new Vector3(2, 2, 2), 0.000001f);

        Assert.Equal(transforms[0].position, new Vector3(1, 1, 1));
        Assert.Equal(transforms[1].position, new Vector3(2, 2, 2));
        Assert.Equal(transforms[2].position, new Vector3(3, 3, 3));
        Assert.Equal(transforms[3].position, new Vector3(1, 1, 1));
    }

    [Test]
    public void CancelToken()
    {
        var cts = new CancellationTokenSource();
        var op = InstantiateAsync(emptyPrefab, 3, new InstantiateParameters(), cts.Token);
        cts.Cancel();
        Assert.True(cts.IsCancellationRequested);
        Assert.True(cts.Token.WaitHandle.WaitOne(0));
        Assert.Null(op.Result);
    }

    [Test]
    public void CancelOp()
    {
        var op = InstantiateAsync(emptyPrefab, 3, new InstantiateParameters());
        op.Cancel();
        Assert.Null(op.Result);
    }

    [Test]
    public IEnumerator<TestYield> InstantiateParameters()
    {
        var parent = new GameObject().transform;
        var op = InstantiateAsync(emptyPrefab, 3, new InstantiateParameters { parent = parent });
        yield return new UnityYield(op);

        Assert.Equal(op.Result.Length, 3);

        foreach (var obj in op.Result)
        {
            Assert.Equal(obj.transform.parent, parent);
        }
    }
}
