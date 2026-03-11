using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public class ResourceAsync__2022_3__6000_0_44f1 : MonoBehaviour
{
    public ITestAsset emptyObjectAsset => new GameObjectAsset();

    [TestInjectResource(nameof(emptyObjectAsset))]
    public OnceOnlyPath loadResource;

    [Test]
    public IEnumerator<TestYield> Load()
    {
        var callback = false;
        // note that resource.asset access will force load
        var resource = Resources.LoadAsync(loadResource);
        resource.completed += _ => callback = true;
        Assert.False(callback);
        yield return new UnityYield(new WaitForEndOfFrame());
        Assert.False(callback);
        yield return new UnityYield(null);
        Assert.True(callback);
        Assert.NotNull(resource.asset);
    }

    [TestInjectResource(nameof(emptyObjectAsset))]
    public OnceOnlyPath loadResourceYield;

    [Test]
    public IEnumerator<TestYield> LoadYield()
    {
        var startTime = Time.frameCount;
        var callback = false;
        // note that resource.asset access will force load
        var resource = Resources.LoadAsync(loadResourceYield);
        resource.completed += _ => callback = true;
        Assert.False(resource.isDone);
        Assert.False(callback);
        yield return new UnityYield(resource);
        Assert.Equal(1, Time.frameCount - startTime);
        Assert.True(resource.isDone);
        Assert.False(callback);
        yield return new UnityYield(new WaitForEndOfFrame());
        Assert.Equal(1, Time.frameCount - startTime);
        Assert.True(resource.isDone);
        Assert.True(callback);
        yield return new UnityYield(null);
        Assert.True(resource.isDone);
        Assert.True(callback);
        Assert.NotNull(resource.asset);
    }

    [TestInjectResource(nameof(emptyObjectAsset))]
    public OnceOnlyPath loadResourceBlocking;

    [Test]
    public IEnumerator<TestYield> LoadBlocking()
    {
        var callback = false;
        var resource = Resources.LoadAsync(loadResourceBlocking);
        resource.completed += _ => callback = true;
        Assert.False(callback);
        // force load
        Assert.NotNull(resource.asset);
        Assert.False(callback);
        yield return new UnityYield(new WaitForEndOfFrame());
        Assert.False(callback);
        yield return new UnityYield(null);
        Assert.True(callback);
    }

    [Test(InitTestTiming.Awake)]
    public IEnumerator<TestYield> UnloadUnusedAssetsAwake()
    {
        var startTime = Time.frameCount;
        var unloadUnused = Resources.UnloadUnusedAssets();
        var callback = false;
        unloadUnused.completed += _ => callback = true;
        yield return new UnityYield(unloadUnused);
        Assert.Equal(1, Time.frameCount - startTime);
        Assert.False(callback);
        yield return new UnityYield(new WaitForEndOfFrame());
        Assert.True(callback);
    }
}
