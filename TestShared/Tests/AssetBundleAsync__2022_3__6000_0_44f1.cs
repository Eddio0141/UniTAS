using System.Collections.Generic;
using UnityEngine;

public class AssetBundleAsync__2022_3__6000_0_44f1 : MonoBehaviour
{
    public ITestAsset emptyObjectAsset => new GameObjectAsset();

    [TestInjectResource(nameof(emptyObjectAsset))]
    public OnceOnlyPath loadOnInitBlockingAwakeResource;

    public Dictionary<string, ITestAsset> assetBundleEmptyObjs => new()
    {
        { "empty.prefab", new GameObjectAsset() },
        { "empty2.prefab", new GameObjectAsset() }
    };

    [TestInjectAssetBundle(nameof(assetBundleEmptyObjs))]
    public OnceOnlyPath loadOnInitNonBlockingAssetBundle;

    [Test(InitTestTiming.Awake)]
    public void LoadOnInitBlockingAwake()
    {
        var resource = Resources.LoadAsync(loadOnInitBlockingAwakeResource);

        var op = AssetBundle.LoadFromFileAsync(loadOnInitNonBlockingAssetBundle);
        var callback = false;
        op.completed += _ => callback = true;

        // touching the property will force it to load
        var asset = op.assetBundle;
        Assert.NotNull(asset);
        Assert.True(op.isDone);
        Assert.True(callback);
        // resource will also be forced to load, despite not being touched
        Assert.True(resource.isDone);
    }
}
