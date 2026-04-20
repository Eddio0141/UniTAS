using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AssetBundleAsync__2022_3__6000_0_44f1 : MonoBehaviour
{
    public ITestAsset emptyObjectAsset => new GameObjectAsset();

    [TestInjectResource(nameof(emptyObjectAsset))]
    public OnceOnlyPath blockingLoadResource;

    public Dictionary<string, ITestAsset> assetBundleEmpty => new();

    public Dictionary<string, ITestAsset> assetBundleEmptyObjs => new()
    {
        { "empty.prefab", new GameObjectAsset() },
        { "empty2.prefab", new GameObjectAsset() }
    };

    [TestInjectAssetBundle(nameof(assetBundleEmptyObjs))]
    public OnceOnlyPath blockingLoadBundle;

    [TestInjectAssetBundle(nameof(assetBundleEmptyObjs))]
    public OnceOnlyPath blockingLoadBundle2;

    [TestInjectPrefab(nameof(emptyObjectAsset))] public GameObject emptyPrefab;

    [TestInjectScene] public string scenePath;

    [Test(InitTestTiming.Awake)]
    public void BlockingLoad()
    {
        var resource = Resources.LoadAsync(blockingLoadResource);
        var prefab = InstantiateAsync(emptyPrefab);
        var scene = SceneManager.LoadSceneAsync(scenePath);
        var anotherop = AssetBundle.LoadFromFileAsync(blockingLoadBundle2);

        var op = AssetBundle.LoadFromFileAsync(blockingLoadBundle);

        Assert.False(resource.isDone);
        Assert.False(prefab.isDone);
        Assert.False(scene.isDone);
        Assert.False(anotherop.isDone);

        // force load assetBundle
        var asset = op.assetBundle;
        Assert.NotNull(asset);
        Assert.True(op.isDone);

        // which other async ops are forced to load?
#if !UNITY_EDITOR
        // these cannot work in a non-deterministic environment such as the editor runner, or just unmodded unity
        Assert.True(resource.isDone);
        Assert.True(scene.isDone);
        Assert.True(anotherop.isDone);
#endif
        Assert.False(prefab.isDone);
    }

    [TestInjectAssetBundle(nameof(assetBundleEmptyObjs))]
    public OnceOnlyPath blockingLoadResourceUnloadBundle;

    [Test(InitTestTiming.Awake)]
    public void BlockingLoadResourceUnload()
    {
        var resource = Resources.UnloadUnusedAssets();

        var op = AssetBundle.LoadFromFileAsync(blockingLoadResourceUnloadBundle);

        Assert.False(resource.isDone);

        // force load assetBundle
        var asset = op.assetBundle;
        Assert.NotNull(asset);
        Assert.True(op.isDone);

        // is op forced to load
        Assert.True(resource.isDone);
    }

    [TestInjectAssetBundle(nameof(assetBundleEmptyObjs))]
    public OnceOnlyPath blockingLoadReverseBundle;

    [TestInjectAssetBundle(nameof(assetBundleEmptyObjs))]
    public OnceOnlyPath blockingLoadReverseBundle2;

    [TestInjectResource(nameof(emptyObjectAsset))]
    public OnceOnlyPath blockingLoadReverseResource;

    [Test(InitTestTiming.Awake)]
    public void BlockingLoadReverse()
    {
        var op = AssetBundle.LoadFromFileAsync(blockingLoadReverseBundle);

        var resource = Resources.LoadAsync(blockingLoadReverseResource);
        var prefab = InstantiateAsync(emptyPrefab);
        var scene = SceneManager.LoadSceneAsync(scenePath);
        var anotherop = AssetBundle.LoadFromFileAsync(blockingLoadReverseBundle2);

        Assert.False(resource.isDone);
        Assert.False(prefab.isDone);
        Assert.False(scene.isDone);
        Assert.False(anotherop.isDone);

        // force load assetBundle
        var asset = op.assetBundle;
        Assert.NotNull(asset);
        Assert.True(op.isDone);

        // which other async ops are forced to load?
#if !UNITY_EDITOR
        // these cannot work in a non-deterministic environment such as the editor runner, or just unmodded unity
        Assert.True(resource.isDone);
        Assert.True(scene.isDone);
        Assert.True(anotherop.isDone);
#endif
        Assert.False(prefab.isDone);
    }

    [TestInjectAssetBundle(nameof(assetBundleEmptyObjs))]
    public OnceOnlyPath blockingLoadResourceUnloadReverseBundle;

    [Test(InitTestTiming.Awake)]
    public void BlockingLoadResourceUnloadReverse()
    {
        var op = AssetBundle.LoadFromFileAsync(blockingLoadResourceUnloadReverseBundle);

        var resource = Resources.UnloadUnusedAssets();

        Assert.False(resource.isDone);

        // force load assetBundle
        var asset = op.assetBundle;
        Assert.NotNull(asset);
        Assert.True(op.isDone);

        // is op forced to load
        Assert.True(resource.isDone);
    }

    [TestInjectAssetBundle(nameof(assetBundleEmptyObjs))]
    public OnceOnlyPath loadFromMemoryAsyncBundle;

    [Test(InitTestTiming.Awake)]
    public IEnumerator<TestYield> LoadFromMemoryAsync()
    {
        var resourceRaw = File.ReadAllBytes(loadFromMemoryAsyncBundle);
        var op = AssetBundle.LoadFromMemoryAsync(resourceRaw);

        Assert.False(op.isDone);

        yield return new UnityYield(null);

        Assert.True(op.isDone);
        var asset = op.assetBundle;
        Assert.NotNull(asset);
    }

    [TestInjectAssetBundle(nameof(assetBundleEmptyObjs))]
    public OnceOnlyPath loadFromStreamAsyncBundle;

#if !UNITY_EDITOR
    [Test(InitTestTiming.Awake)]
    public IEnumerator<TestYield> LoadFromStreamAsync()
    {
        var resourceRaw = File.Open(loadFromStreamAsyncBundle, FileMode.Open);
        var op = AssetBundle.LoadFromStreamAsync(resourceRaw);

        Assert.False(op.isDone);

        yield return new UnityYield(null);

        Assert.True(op.isDone);
        var asset = op.assetBundle;
        Assert.NotNull(asset);
    }
#endif

    [TestInjectAssetBundle(nameof(assetBundleEmptyObjs))]
    public OnceOnlyPath unloadAsyncBundle;

    [Test(InitTestTiming.Awake)]
    public IEnumerator<TestYield> UnloadAsync()
    {
        var op = AssetBundle.LoadFromFileAsync(unloadAsyncBundle);
        yield return new UnityYield(op);
        Assert.True(op.isDone);

        // load asset
        var bundle = op.assetBundle;
        var obj = bundle.LoadAsset<GameObject>("empty.prefab");

        // destroy objects too!
        var unloadOp = bundle.UnloadAsync(true);

        Assert.False(unloadOp.isDone);
        Assert.True(obj);
        yield return new UnityYield(null);
        Assert.True(unloadOp.isDone);
        Assert.False(obj);
    }

    [TestInjectAssetBundle(nameof(assetBundleEmptyObjs))]
    public OnceOnlyPath getAssetBundle;

    [Test(InitTestTiming.Awake)]
    public IEnumerator<TestYield> GetAsset()
    {
        var op = AssetBundle.LoadFromFileAsync(getAssetBundle);
        yield return new UnityYield(op);
        Assert.True(op.isDone);

        var bundle = op.assetBundle;
        var loadAsset = bundle.LoadAssetAsync("empty.prefab");

        Assert.False(loadAsset.isDone);
        yield return new UnityYield(null);
        Assert.True(loadAsset.isDone);

        Assert.Equal(loadAsset.asset.name, "empty");
        Assert.Equal(loadAsset.allAssets.Length, 1);
        Assert.Equal(loadAsset.allAssets[0].name, "empty");
    }

    [TestInjectAssetBundle(nameof(assetBundleEmptyObjs))]
    public OnceOnlyPath loadAssetWithSubAssetsAsyncBundle;

    [Test(InitTestTiming.Awake)]
    public IEnumerator<TestYield> LoadAssetWithSubAssetsAsync()
    {
        var op = AssetBundle.LoadFromFileAsync(loadAssetWithSubAssetsAsyncBundle);
        yield return new UnityYield(op);
        Assert.True(op.isDone);

        var bundle = op.assetBundle;
        var loadAsset = bundle.LoadAssetWithSubAssetsAsync("empty.prefab");

        Assert.False(loadAsset.isDone);
        yield return new UnityYield(null);
        Assert.True(loadAsset.isDone);

        Assert.Equal(loadAsset.asset.name, "empty");
        Assert.Equal(loadAsset.allAssets.Length, 1);
        Assert.Equal(loadAsset.allAssets[0].name, "empty");
    }
}
