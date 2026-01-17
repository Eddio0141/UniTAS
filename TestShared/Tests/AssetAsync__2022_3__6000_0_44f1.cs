using System.Collections.Generic;
using UnityEngine;

public class AssetAsync__2022_3_6000_0_4 : MonoBehaviour
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

    [TestInjectResource(nameof(emptyObjectAsset))]
    public OnceOnlyPath loadResource;

    [Test]
    public IEnumerator<TestYield> LoadResource()
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
    public IEnumerator<TestYield> LoadResourceYield()
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
    public IEnumerator<TestYield> LoadResourceBlocking()
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

    // public static AsyncOperation LoadEmpty2;
    //
    // private bool _testCoroutineBundleYield;
    // private AssetBundleCreateRequest _yieldBundleLoad;
    //
    // private IEnumerator TestCoroutine()
    // {
    //     var testLoad = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "test"));
    //     var time4 = Time.frameCount;
    //     testLoad.completed += _ => { Assert.Equal("asset_bundle.load_time", 1, Time.frameCount - time4); };
    //     _yieldBundleLoad = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "test2"));
    //     var time = Time.frameCount;
    //     _yieldBundleLoad.completed += _ => { Assert.Equal("asset_bundle.load_time", 1, Time.frameCount - time); };
    //     yield return _yieldBundleLoad;
    //     Assert.True("asset_bundle.op.isDone", testLoad.isDone);
    //     Assert.True("asset_bundle.op.isDone", _yieldBundleLoad.isDone);
    //     testLoad.assetBundle.Unload(true);
    //     _testCoroutineBundleYield = true;
    //     var bundleGet = _yieldBundleLoad.assetBundle.LoadAssetAsync("Dummy2");
    //     var time2 = Time.frameCount;
    //     bundleGet.completed += _ => { Assert.Equal("asset_bundle.load_asset.load_time", 1, Time.frameCount - time2); };
    //     yield return bundleGet;
    //     Assert.NotNull("asset_bundle.load_asset.asset", bundleGet.asset);
    //     Assert.Equal("asset_bundle.load_asset.asset", bundleGet.asset, bundleGet.allAssets[0]);
    //     var time3 = Time.frameCount;
    //     var bundleGetAll = _yieldBundleLoad.assetBundle.LoadAllAssetsAsync();
    //     bundleGetAll.completed += _ =>
    //     {
    //         Assert.Equal("asset_bundle.load_all_asset.load_time", 1, Time.frameCount - time3);
    //     };
    //     yield return bundleGetAll;
    //     Assert.NotNull("asset_bundle.load_all_asset.assets", bundleGet.allAssets);
    //     Assert.Equal("asset_bundle.load_all_asset.assets", 1, bundleGet.allAssets.Length);
    //     Assert.NotNull("asset_bundle.load_all_asset.assets", bundleGetAll.allAssets);
    //     Assert.NotNull("asset_bundle.load_asset.asset", bundleGetAll.asset);
    //     Assert.Equal("asset_bundle.load_asset.asset", bundleGet.asset.name, "Dummy2");
    //     var bundleGet2 = _yieldBundleLoad.assetBundle.LoadAssetAsync("Dummy4");
    //     yield return bundleGet2;
    //     Assert.Equal("asset_bundle.load_asset.asset", bundleGet.allAssets.Length, 1);
    //     Assert.Equal("asset_bundle.load_asset.asset", bundleGet2.allAssets.Length, 1);
    // }
    //
    // private IEnumerator TestCoroutine2()
    // {
    //     var testLoad = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "test3"));
    //     var time = Time.frameCount;
    //     testLoad.completed += _ => { Assert.Equal("asset_bundle.load_time", 1, Time.frameCount - time); };
    //     yield return testLoad;
    //     Assert.True("asset_bundle.op.isDone", testLoad.isDone);
    //
    //     testLoad.assetBundle.Unload(true);
    // }
    //
    // private IEnumerator Start()
    // {
    //     var coroutine1 = StartCoroutine(TestCoroutine());
    //     var coroutine2 = StartCoroutine(TestCoroutine2());
    //     yield return coroutine1;
    //     yield return coroutine2;
    //
    //     var unscaledTime = Time.unscaledTime;
    //     var scaledTime = Time.time;
    //     Time.timeScale = 0f;
    //
    //     Assert.True("asset_bundle.op.isDone", _yieldBundleLoad.isDone);
    //     Assert.True("coroutine.managed_async_op.yield", _testCoroutineBundleYield);
    //
    //     _yieldBundleLoad.assetBundle.Unload(true);
    //
    //     // StructTest
    //     Assert.NotThrows("struct.constrained_opcode", () => _ = new StructTest("bar"));
    //
    //     var startFrame = Time.frameCount;
    //
    //     // frame 1
    //     var loadEmpty = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     Assert.Equal("scene.op.priority", 0, loadEmpty.priority);
    //     var bundleLoad = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "test"));
    //     var bundleLoadTime = Time.frameCount;
    //     bundleLoad.completed += _ =>
    //     {
    //         Assert.Equal("asset_bundle.op.callback_frame", 2, Time.frameCount - bundleLoadTime);
    //     };
    //     var emptyScene = SceneManager.GetSceneAt(1);
    //     var emptySceneByName = SceneManager.GetSceneByName("Empty");
    //     Assert.Equal("scene.op.progress", 0.9f, loadEmpty.progress, 0.0001f);
    //     Assert.False("scene.op.isDone", loadEmpty.isDone);
    //
    //     Assert.Throws("scene.get_scene_at.set_name",
    //         new InvalidOperationException(
    //             "Setting a name on a saved scene is not allowed (the filename is used as name). Scene: 'Assets/Scenes/Empty.unity'"),
    //         () => emptyScene.name = "foo");
    //
    //     Assert.Equal("scene.sceneCount", 2, SceneManager.sceneCount);
    //     Assert.Equal("scene.loadedSceneCount", 1, SceneManager.loadedSceneCount);
    //     loadEmpty.allowSceneActivation = false;
    //     loadEmpty.completed += _ =>
    //     {
    //         // frame 3
    //         // sceneCount to get count including loading / unloading
    //         Assert.Equal("scene.sceneCount", 2, SceneManager.sceneCount);
    //         Assert.Equal("scene.loadedSceneCount", 2, SceneManager.loadedSceneCount);
    //         Assert.Equal("scene.op.callback_time", 2, Time.frameCount - startFrame);
    //
    //         var actualScene = SceneManager.GetSceneAt(1);
    //         Assert.True("scene.dummy_scene_struct.eq", emptyScene == actualScene);
    //         Assert.False("scene.dummy_scene_struct.neq", emptyScene != actualScene);
    //         Assert.True("scene.dummy_scene_struct.Equals", emptyScene.Equals(actualScene));
    //         Assert.Equal("scene.dummy_scene_struct.name", "Empty", emptyScene.name);
    //         Assert.True("scene.dummy_scene_struct.isLoaded", emptyScene.isLoaded);
    //         Assert.Equal("scene.dummy_scene_struct.rootCount", 1, emptyScene.rootCount);
    //         Assert.True("scene.dummy_scene_struct.isSubScene", emptyScene.isSubScene);
    //         Assert.Equal("scene.dummy_scene_struct.path", "Assets/Scenes/Empty.unity", emptyScene.path);
    //         Assert.Equal("scene.dummy_scene_struct.buildIndex", 3, emptyScene.buildIndex);
    //         Assert.False("scene.dummy_scene_struct.isDirty", emptyScene.isDirty);
    //         Assert.True("scene.dummy_scene_struct.IsValid", emptyScene.IsValid());
    //         Assert.NotEqual("scene.dummy_scene_struct.handle", 0, emptyScene.handle);
    //         Assert.NotEqual("scene.dummy_scene_struct.hash_code", 0, emptyScene.GetHashCode());
    //     };
    //
    //     Assert.Throws("scene.dummy_scene_struct.set_active",
    //         new ArgumentException(
    //             "SceneManager.SetActiveScene failed; scene 'Empty' is not loaded and therefore cannot be set active"),
    //         () => SceneManager.SetActiveScene(emptyScene));
    //
    //     var emptyScene2 = SceneManager.GetSceneByName("Empty");
    //
    //     Assert.Equal("scene.dummy_scene_struct.eq", emptyScene, emptyScene2);
    //     Assert.Throws("scene.dummy_scene_struct.set_active",
    //         new ArgumentException(
    //             "SceneManager.SetActiveScene failed; scene 'Empty' is not loaded and therefore cannot be set active"),
    //         () => SceneManager.SetActiveScene(emptyScene2));
    //
    //     var emptyScene3 = SceneManager.GetSceneByPath("Assets/Scenes/Empty.unity");
    //
    //     Assert.Throws("scene.dummy_scene_struct.set_active",
    //         new ArgumentException(
    //             "SceneManager.SetActiveScene failed; scene 'Empty' is not loaded and therefore cannot be set active"),
    //         () => SceneManager.SetActiveScene(emptyScene3));
    //
    //     yield return null;
    //
    //     // 1f passed, unitas forces 100 fps by default
    //     Assert.Equal("time.unscaled_time", 0.01f, Time.unscaledTime - unscaledTime, 0.001f);
    //     Assert.Equal("time.time", 0f, Time.time - scaledTime, 0.001f);
    //     Time.timeScale = 1;
    //
    //     Assert.False("scene.op.isDone", loadEmpty.isDone);
    //     Assert.False("asset_bundle.op.isDone", bundleLoad.isDone);
    //     Assert.Equal("asset_bundle.op.progress", 0.9f, bundleLoad.progress, 0.0001f);
    //
    //     // frame 2
    //     // loadEmpty 1f delay
    //
    //     loadEmpty.allowSceneActivation = true;
    //     Assert.Equal("scene.sceneCount", 2, SceneManager.sceneCount);
    //     Assert.Equal("scene.loadedSceneCount", 1, SceneManager.loadedSceneCount);
    //
    //     yield return null;
    //     Assert.Equal("time.unscaled_time", 0.02f, Time.unscaledTime - unscaledTime, 0.001f);
    //     Assert.Equal("time.time", 0.01f, Time.time - scaledTime, 0.001f);
    //
    //     Assert.True("scene.op.isDone", loadEmpty.isDone);
    //     Assert.Equal("scene.op.progress", 1f, loadEmpty.progress, 0.0001f);
    //     Assert.True("asset_bundle.op.isDone", bundleLoad.isDone);
    //     var bundleRequestStart = Time.frameCount;
    //     var bundleRequest = bundleLoad.assetBundle.LoadAssetAsync<GameObject>("Dummy");
    //     bundleRequest.completed += _ =>
    //     {
    //         Assert.Equal("asset_bundle_request.op.done_frame", 1, Time.frameCount - bundleRequestStart);
    //     };
    //
    //     Assert.False("asset_bundle_request.op.isDone", bundleRequest.isDone);
    //
    //     yield return null;
    //
    //     Assert.True("asset_bundle_request.op.isDone", bundleRequest.isDone);
    //     Assert.Equal("asset_bundle_request.asset", new Vector3(1, 2, 3),
    //         (bundleRequest.asset as GameObject)?.transform.position);
    //
    //     bundleRequest = bundleLoad.assetBundle.LoadAllAssetsAsync();
    //     Assert.False("asset_bundle_request.op.isDone", bundleRequest.isDone);
    //
    //     yield return null;
    //
    //     Assert.True("asset_bundle_request.op.isDone", bundleRequest.isDone);
    //
    //     bundleLoad.assetBundle.Unload(true);
    //
    //     var emptyScene4 = SceneManager.GetSceneAt(1);
    //     Assert.Equal("scene.dummy_scene_struct.eq_real", emptyScene, emptyScene4);
    //
    //     var general = SceneManager.GetActiveScene();
    //     Assert.True("scene.dummy_scene_struct.set_active", SceneManager.SetActiveScene(emptyScene));
    //     SceneManager.SetActiveScene(general);
    //     Assert.True("scene.dummy_scene_struct.set_active", SceneManager.SetActiveScene(emptyScene2));
    //     SceneManager.SetActiveScene(general);
    //     Assert.True("scene.dummy_scene_struct.set_active", SceneManager.SetActiveScene(emptyScene3));
    //
    //     // frame 3
    //
    //     Assert.Equal("scene.sceneCount", 2, SceneManager.sceneCount);
    //     Assert.Equal("scene.loadedSceneCount", 2, SceneManager.loadedSceneCount);
    //
    //     var unloadEmpty = SceneManager.UnloadSceneAsync("Empty")!;
    //     Assert.Equal("scene.sceneCount", 2, SceneManager.sceneCount);
    //     Assert.Equal("scene.loadedSceneCount", 1, SceneManager.loadedSceneCount);
    //     Assert.Equal("scene.unload_op.progress", 0f, unloadEmpty.progress, 0.0001f);
    //     Assert.False("scene.unload_op.isDone", unloadEmpty.isDone);
    //     var startFrame3 = Time.frameCount;
    //     unloadEmpty.completed += _ =>
    //     {
    //         // frame 4
    //         Assert.Equal("scene.sceneCount", 1, SceneManager.sceneCount);
    //         Assert.Equal("scene.loadedSceneCount", 1, SceneManager.loadedSceneCount);
    //         Assert.Equal("scene.unload.frame", 1, Time.frameCount - startFrame3);
    //     };
    //
    //     yield return null;
    //     // frame 4
    //
    //     Assert.Equal("scene.unload_op.progress", 1f, unloadEmpty.progress, 0.0001f);
    //     Assert.True("scene.unload_op.isDone", unloadEmpty.isDone);
    //     Assert.Equal("scene.sceneCount", 1, SceneManager.sceneCount);
    //     Assert.Equal("scene.loadedSceneCount", 1, SceneManager.loadedSceneCount);
    //
    //     yield return null;
    //     // frame 5
    //
    //     loadEmpty = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     var startFrame4 = Time.frameCount;
    //     loadEmpty.completed += _ =>
    //     {
    //         // frame 7
    //         Assert.Equal("scene.load.frame", 2, Time.frameCount - startFrame4);
    //     };
    //
    //     yield return null;
    //     // frame 6
    //
    //     yield return null;
    //     // frame 7
    //
    //     loadEmpty = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     var startFrame5 = Time.frameCount;
    //     loadEmpty.completed += _ =>
    //     {
    //         // frame 10
    //         Assert.Equal("scene.load.frame", 3, Time.frameCount - startFrame5);
    //     };
    //
    //     yield return null;
    //     // frame 8
    //     // loadEmpty 1f delay
    //     loadEmpty.allowSceneActivation = false; // doing this would already have the 1f delay erased
    //
    //     yield return null;
    //     // frame 9
    //     loadEmpty.allowSceneActivation = true;
    //
    //     yield return null;
    //     // frame 10
    //
    //     loadEmpty = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     bundleLoad = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "test"));
    //     bundleLoad.priority = 123;
    //     Assert.Equal("op.priority", 123, bundleLoad.priority);
    //     loadEmpty.allowSceneActivation = false;
    //     var startFrame6 = Time.frameCount;
    //     loadEmpty.completed += _ =>
    //     {
    //         // frame 15
    //         Assert.Equal("scene.load.frame", 5, Time.frameCount - startFrame6);
    //     };
    //
    //     yield return null;
    //     // frame 11
    //
    //     Assert.False("asset_bundle.op.isDone", bundleLoad.isDone);
    //     yield return null;
    //     // frame 12
    //     Assert.False("asset_bundle.op.isDone", bundleLoad.isDone);
    //     yield return null;
    //     // frame 13
    //     Assert.False("asset_bundle.op.isDone", bundleLoad.isDone);
    //     yield return null;
    //     // frame 14
    //     loadEmpty.allowSceneActivation = true;
    //     Assert.False("asset_bundle.op.isDone", bundleLoad.isDone);
    //
    //     yield return null;
    //     // frame 15
    //
    //     Assert.True("asset_bundle.op.isDone", bundleLoad.isDone);
    //     bundleLoad.assetBundle.Unload(true);
    //
    //     loadEmpty = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     loadEmpty.allowSceneActivation = false;
    //     var startFrame7 = Time.frameCount;
    //     loadEmpty.completed += _ =>
    //     {
    //         // frame 16
    //         Assert.Equal("scene.load.frame", 1, Time.frameCount - startFrame7);
    //     };
    //     SceneManager.LoadScene("Empty", LoadSceneMode.Additive);
    //     loadEmpty = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     loadEmpty.allowSceneActivation = false;
    //     loadEmpty.completed += _ =>
    //     {
    //         // frame 16
    //         Assert.Equal("scene.load.frame", 1, Time.frameCount - startFrame7);
    //     };
    //
    //     yield return null;
    //     // frame 16
    //
    //     yield return null;
    //     // frame 17
    //
    //     loadEmpty = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     var startFrame8 = Time.frameCount;
    //     loadEmpty.completed += _ =>
    //     {
    //         // frame 19
    //         Assert.Equal("scene.load.frame", 2, Time.frameCount - startFrame8);
    //     };
    //
    //     yield return null;
    //     // frame 18
    //
    //     yield return null;
    //     // frame 19
    //
    //     Assert.Equal("scene.sceneCount", 8, SceneManager.sceneCount);
    //     Assert.Equal("scene.loadedSceneCount", 8, SceneManager.loadedSceneCount);
    //
    //     // multiple unload at the same time conflicts, and only 1 unloads
    //     SceneManager.UnloadSceneAsync("Empty");
    //     Assert.Null("scene.unload.invalid", SceneManager.UnloadSceneAsync("Empty"));
    //     var sceneCount = SceneManager.sceneCount;
    //     Assert.False("scene.scene_struct.isLoaded", SceneManager.GetSceneAt(1).isLoaded);
    //     var firstEmpty = true;
    //     for (var i = 0; i < sceneCount; i++)
    //     {
    //         var scene = SceneManager.GetSceneAt(i);
    //         if (scene.name != "Empty") continue;
    //
    //         if (firstEmpty)
    //         {
    //             firstEmpty = false;
    //             Assert.Null("scene.unload.invalid", SceneManager.UnloadSceneAsync(scene));
    //         }
    //         else
    //             Assert.NotNull("scene.unload.valid", SceneManager.UnloadSceneAsync(scene));
    //     }
    //
    //     yield return null;
    //     // frame 20
    //
    //     Assert.Equal("scene.sceneCount", 1, SceneManager.sceneCount);
    //     Assert.Equal("scene.loadedSceneCount", 1, SceneManager.loadedSceneCount);
    //
    //     SceneManager.LoadScene("Empty", LoadSceneMode.Additive);
    //
    //     yield return null;
    //
    //     SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));
    //     Assert.Null("scene.unload.invalid", SceneManager.UnloadSceneAsync("Empty"));
    //
    //     yield return null;
    //
    //     SceneManager.LoadScene("Empty", LoadSceneMode.Additive);
    //     SceneManager.LoadScene("Empty", LoadSceneMode.Additive);
    //
    //     yield return null;
    //
    //     SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));
    //     Assert.Null("scene.unload.valid", SceneManager.UnloadSceneAsync("Empty"));
    //
    //     yield return null;
    //
    //     SceneManager.LoadScene("Empty", LoadSceneMode.Additive);
    //     SceneManager.LoadScene("Empty", LoadSceneMode.Additive);
    //     SceneManager.LoadScene("Empty", LoadSceneMode.Additive);
    //     SceneManager.LoadScene("Empty2", LoadSceneMode.Additive);
    //
    //     SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!.completed += _ => throw new Exception("foo");
    //
    //     yield return null;
    //
    //     unloadEmpty = SceneManager.UnloadSceneAsync("Empty2")!;
    //     var emptyUnloadFinish = false;
    //     var empty2UnloadFinish = false;
    //     unloadEmpty.completed += _ =>
    //     {
    //         // frame 23
    //         empty2UnloadFinish = true;
    //         Assert.False("scene.op_order", emptyUnloadFinish);
    //     };
    //     unloadEmpty = SceneManager.UnloadSceneAsync("Empty")!;
    //     unloadEmpty.completed += _ =>
    //     {
    //         emptyUnloadFinish = true;
    //         // frame 23
    //     };
    //
    //     yield return null;
    //     // frame 23
    //
    //     Assert.True("scene.op.completed_callback", emptyUnloadFinish);
    //     Assert.True("scene.op.completed_callback", empty2UnloadFinish);
    //
    //     // try unload name and id
    //     SceneManager.UnloadSceneAsync("Empty");
    //     // empty is id 3
    //     Assert.Null("scene.unload.missing", SceneManager.UnloadSceneAsync(3));
    //
    //     yield return null;
    //     // frame 24
    //
    //     var prevSceneCount = SceneManager.sceneCount;
    //
    //     // try load / unload non-existent scene
    //     Assert.Log("scene.load.missing", LogType.Error,
    //         "Scene 'InvalidScene' couldn't be loaded because it has not been added to the build settings or the AssetBundle has not been loaded." +
    //         Environment.NewLine +
    //         "To add a scene to the build settings use the menu File->Build Settings...");
    //     // ReSharper disable once Unity.LoadSceneUnexistingScene
    //     loadEmpty = SceneManager.LoadSceneAsync("InvalidScene", LoadSceneMode.Additive);
    //     Assert.Null("scene.unload.missing", loadEmpty);
    //     Assert.Equal("scene.sceneCount", 0, SceneManager.sceneCount - prevSceneCount);
    //
    //     Assert.Log("scene.load.missing", LogType.Error,
    //         "Scene 'InvalidScene' couldn't be loaded because it has not been added to the build settings or the AssetBundle has not been loaded." +
    //         Environment.NewLine +
    //         "To add a scene to the build settings use the menu File->Build Settings...");
    //     // ReSharper disable once Unity.LoadSceneUnexistingScene
    //     SceneManager.LoadScene("InvalidScene", LoadSceneMode.Additive);
    //     Assert.Equal("scene.sceneCount", 0, SceneManager.sceneCount - prevSceneCount);
    //
    //     Assert.Throws("scene.unload.missing", new ArgumentException("Scene to unload is invalid"),
    //         () => SceneManager.UnloadSceneAsync("InvalidScene"));
    //     // unload scene that was never touched
    //     Assert.Throws("scene.unload.missing", new ArgumentException("Scene to unload is invalid"),
    //         () => SceneManager.UnloadSceneAsync(1));
    //
    //     loadEmpty = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     Assert.Equal("scene.op.progress", 0.9f, loadEmpty.progress, 0.0001f);
    //     var loadEmpty2 = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     Assert.Equal("scene.op.progress", 0.9f, loadEmpty2.progress, 0.0001f);
    //     unloadEmpty = SceneManager.UnloadSceneAsync("Empty")!;
    //     bundleLoad = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "test"));
    //
    //     yield return null;
    //     // frame 25
    //
    //     Assert.Equal("scene.op.progress", 0.9f, loadEmpty.progress, 0.0001f);
    //     Assert.Equal("scene.op.progress", 0.9f, loadEmpty2.progress, 0.0001f);
    //     Assert.False("scene.op.isDone", unloadEmpty.isDone);
    //     Assert.False("asset_bundle.op.isDone", bundleLoad.isDone);
    //
    //     yield return null;
    //     // frame 26
    //
    //     Assert.True("scene.op.isDone", loadEmpty.isDone);
    //     Assert.False("scene.op.isDone", loadEmpty2.isDone);
    //     Assert.False("scene.op.isDone", unloadEmpty.isDone);
    //     Assert.False("asset_bundle.op.isDone", bundleLoad.isDone);
    //
    //     yield return null;
    //     // frame 27
    //
    //     Assert.True("scene.op.isDone", loadEmpty2.isDone);
    //     Assert.True("scene.op.isDone", unloadEmpty.isDone);
    //     Assert.True("asset_bundle.op.isDone", bundleLoad.isDone);
    //     bundleLoad.assetBundle.Unload(true);
    //
    //     yield return null;
    //     // frame 28
    //
    //     Assert.True("scene.op.isDone", unloadEmpty.isDone);
    //
    //     SceneManager.LoadScene("Empty2", LoadSceneMode.Additive);
    //
    //     yield return null;
    //
    //     loadEmpty = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     loadEmpty2 = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     unloadEmpty = SceneManager.UnloadSceneAsync("Empty")!;
    //     SceneManager.LoadScene("Empty", LoadSceneMode.Additive);
    //     var unloadEmpty2 = SceneManager.UnloadSceneAsync("Empty2")!;
    //
    //     yield return null;
    //     // frame 29
    //
    //     Assert.True("scene.op.isDone", loadEmpty.isDone);
    //     Assert.True("scene.op.isDone", loadEmpty2.isDone);
    //     Assert.True("scene.op.isDone", unloadEmpty.isDone);
    //     Assert.True("scene.op.isDone", unloadEmpty2.isDone);
    //
    //     SceneManager.LoadScene("Empty2", LoadSceneMode.Additive);
    //
    //     yield return null;
    //
    //     unloadEmpty = SceneManager.UnloadSceneAsync("Empty")!;
    //     unloadEmpty2 = SceneManager.UnloadSceneAsync("Empty2")!;
    //     loadEmpty = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //
    //     yield return null;
    //
    //     Assert.True("scene.op.isDone", unloadEmpty.isDone);
    //     Assert.True("scene.op.isDone", unloadEmpty2.isDone);
    //     Assert.False("scene.op.isDone", loadEmpty.isDone);
    //
    //     yield return null;
    //
    //     Assert.True("scene.op.isDone", loadEmpty.isDone);
    //
    //     SceneManager.LoadScene("Empty", LoadSceneMode.Additive);
    //
    //     yield return null;
    //
    //     loadEmpty = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     loadEmpty.allowSceneActivation = false;
    //     unloadEmpty = SceneManager.UnloadSceneAsync("Empty")!;
    //     loadEmpty2 = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     bundleLoad = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "test"));
    //
    //     yield return null;
    //
    //     Assert.False("scene.op.isDone", unloadEmpty.isDone);
    //     Assert.False("scene.op.isDone", loadEmpty2.isDone);
    //     Assert.False("asset_bundle.op.isDone", bundleLoad.isDone);
    //
    //     yield return null;
    //
    //     Assert.False("scene.op.isDone", unloadEmpty.isDone);
    //     Assert.False("scene.op.isDone", loadEmpty2.isDone);
    //     Assert.False("asset_bundle.op.isDone", bundleLoad.isDone);
    //
    //     yield return null;
    //
    //     Assert.False("scene.op.isDone", unloadEmpty.isDone);
    //     Assert.False("scene.op.isDone", loadEmpty2.isDone);
    //     Assert.False("asset_bundle.op.isDone", bundleLoad.isDone);
    //
    //     yield return null;
    //
    //     Assert.False("scene.op.isDone", unloadEmpty.isDone);
    //     Assert.False("scene.op.isDone", loadEmpty2.isDone);
    //     Assert.False("asset_bundle.op.isDone", bundleLoad.isDone);
    //
    //     loadEmpty.allowSceneActivation = true;
    //
    //     yield return null;
    //
    //     Assert.True("scene.op.isDone", unloadEmpty.isDone);
    //     Assert.False("scene.op.isDone", loadEmpty2.isDone);
    //     Assert.False("asset_bundle.op.isDone", bundleLoad.isDone);
    //
    //     yield return null;
    //
    //     Assert.True("scene.op.isDone", loadEmpty2.isDone);
    //     Assert.True("asset_bundle.op.isDone", bundleLoad.isDone);
    //     bundleLoad.assetBundle.Unload(true);
    //
    //     loadEmpty = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     bundleLoad = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "test"));
    //
    //     yield return null;
    //     loadEmpty.allowSceneActivation = false;
    //
    //     yield return null;
    //     Assert.False("asset_bundle.op.isDone", bundleLoad.isDone);
    //
    //     yield return null;
    //     Assert.False("asset_bundle.op.isDone", bundleLoad.isDone);
    //
    //     yield return null;
    //
    //     loadEmpty.allowSceneActivation = true;
    //     Assert.False("asset_bundle.op.isDone", bundleLoad.isDone);
    //
    //     yield return null;
    //     Assert.True("asset_bundle.op.isDone", bundleLoad.isDone);
    //
    //     loadEmpty = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     var bundleRequestStart2 = Time.frameCount;
    //     bundleRequest = bundleLoad.assetBundle.LoadAssetAsync<GameObject>("Dummy");
    //     Assert.False("asset_bundle_request.op.isDone", bundleRequest.isDone);
    //     bundleRequest.completed += _ =>
    //     {
    //         Assert.Equal("asset_bundle_request.op.done_frame", 2 + 5, Time.frameCount - bundleRequestStart2);
    //     };
    //
    //     loadEmpty.allowSceneActivation = false;
    //
    //     yield return null;
    //
    //     for (var i = 0; i < 5; i++)
    //     {
    //         var bundleRequestStart3 = Time.frameCount;
    //         bundleRequest = bundleLoad.assetBundle.LoadAssetAsync<GameObject>("Dummy");
    //         Assert.False("asset_bundle_request.op.isDone", bundleRequest.isDone);
    //         var j = i;
    //         bundleRequest.completed += _ =>
    //         {
    //             Assert.Equal("asset_bundle_request.op.done_frame", 6 - j, Time.frameCount - bundleRequestStart3);
    //         };
    //
    //         yield return null;
    //     }
    //
    //     loadEmpty.allowSceneActivation = true;
    //
    //     yield return null;
    //
    //     bundleLoad.assetBundle.Unload(true);
    //
    //     bundleLoad = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "test"));
    //     var bundleRequestStart4 = Time.frameCount;
    //     bundleLoad.completed += _ =>
    //     {
    //         Assert.Equal("asset_bundle.load_time", 1, Time.frameCount - bundleRequestStart4);
    //     };
    //
    //     yield return bundleLoad;
    //
    //     Assert.True("asset_bundle.op.isDone", bundleLoad.isDone);
    //     Assert.NotNull("asset_bundle.bundle", bundleLoad.assetBundle);
    //     bundleLoad.assetBundle.Unload(true);
    //
    //     loadEmpty = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     var startFrame9 = Time.frameCount;
    //     var loadEmptyCompleted = false;
    //     loadEmpty.completed += _ =>
    //     {
    //         loadEmptyCompleted = true;
    //         Assert.Equal("scene.load_time", 2, Time.frameCount - startFrame9);
    //     };
    //
    //     yield return loadEmpty;
    //
    //     Assert.False("scene.op.completed_callback", loadEmptyCompleted);
    //     Assert.True("scene.op.isDone", loadEmpty.isDone);
    //
    //     var loadedEmpty = false;
    //     loadEmpty = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     loadEmpty.completed += _ => { loadedEmpty = true; };
    //
    //     yield return loadEmpty;
    //
    //     Assert.False("load_scene_async.op_callback_end_of_frame", loadedEmpty);
    //     yield return new WaitForEndOfFrame();
    //     Assert.True("load_scene_async.op_callback_end_of_frame", loadedEmpty);
    //
    //     loadedEmpty = false;
    //     loadEmpty = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     loadEmpty.completed += _ => { loadedEmpty = true; };
    //     yield return loadEmpty;
    //
    //     // completes instantly
    //     loadEmpty = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     var testLoad = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "test"));
    //     // doesn't complete
    //     var loadEmpty4 = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     fooResource = Resources.LoadAsync("Foo");
    //     Debug.Log(testLoad.assetBundle); // force load (shouldn't work)
    //
    //     Assert.True("scene.op.isDone", loadEmpty.isDone);
    //     Assert.True("asset_bundle.op.isDone", testLoad.isDone);
    //     Assert.False("load_scene_async.op_callback_forced", loadedEmpty);
    //     Assert.False("scene.op.isDone", loadEmpty4.isDone);
    //     Assert.False("resources.op.isDone", fooResource.isDone);
    //
    //     yield return loadEmpty4;
    //     yield return fooResource;
    //
    //     var loadDummy = testLoad.assetBundle.LoadAssetAsync<GameObject>("Dummy");
    //     _ = loadDummy.asset;
    //     Assert.True("load_asset_async.get_asset_op_force", loadDummy.isDone);
    //
    //     loadDummy = testLoad.assetBundle.LoadAllAssetsAsync();
    //     _ = loadDummy.allAssets;
    //     Assert.True("load_asset_async.get_asset_op_force", loadDummy.isDone);
    //
    //     fooResource = Resources.LoadAsync("Foo");
    //     _ = fooResource.asset;
    //     Assert.False("resource_load_async.get_asset_op_force", fooResource.isDone);
    //
    //     testLoad.assetBundle.Unload(true);
    //
    //     SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive);
    //
    //     var unusedUnusedTime2 = Time.frameCount;
    //     unloadUnused = Resources.UnloadUnusedAssets();
    //     unloadUnused.completed += _ =>
    //     {
    //         // scene load + op should take 2f
    //         Assert.Equal("resources.unload_unused.op.done_frame", 2, Time.frameCount - unusedUnusedTime2);
    //     };
    //
    //     yield return unloadUnused;
    //
    //     prevSceneCount = SceneManager.sceneCount;
    //     // ReSharper disable once Unity.LoadSceneUnexistingScene
    //     yield return SceneManager.LoadSceneAsync("Foo/Dummy", LoadSceneMode.Additive);
    //     Assert.Equal("asset_bundle_scene.scene_partial_path", 0, SceneManager.sceneCount - prevSceneCount);
    //
    //     // bundle load with same scene name as built in one
    //     var testSceneBundle = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "testscene"));
    //     yield return testSceneBundle;
    //     yield return SceneManager.LoadSceneAsync("Dummy", LoadSceneMode.Additive);
    //     var sceneInfo = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
    //     Assert.Equal("asset_bundle_scene.duplicate_to_builtin", "Assets/DummyAssets/Foo/Dummy.unity", sceneInfo.path);
    //     Assert.Equal("asset_bundle_scene.duplicate_to_builtin", "Dummy", sceneInfo.name);
    //
    //     // full path
    //     prevSceneCount = SceneManager.sceneCount;
    //     yield return SceneManager.LoadSceneAsync("Scenes/Foo/Dummy", LoadSceneMode.Additive);
    //     sceneInfo = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
    //     Assert.Equal("asset_bundle_scene.duplicate_to_builtin", 1, SceneManager.sceneCount - prevSceneCount);
    //     Assert.Equal("asset_bundle_scene.duplicate_to_builtin", "Assets/Scenes/Foo/Dummy.unity", sceneInfo.path);
    //     Assert.Equal("asset_bundle_scene.duplicate_to_builtin", "Dummy", sceneInfo.name);
    //
    //     yield return SceneManager.LoadSceneAsync("Assets/Scenes/Foo/Dummy.unity", LoadSceneMode.Additive);
    //     sceneInfo = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
    //     Assert.Equal("asset_bundle_scene.duplicate_to_builtin", "Assets/Scenes/Foo/Dummy.unity", sceneInfo.path);
    //     Assert.Equal("asset_bundle_scene.duplicate_to_builtin", "Dummy", sceneInfo.name);
    //
    //     // those fail to load
    //
    //     // ReSharper disable once Unity.LoadSceneUnexistingScene
    //     var loadDummyScene = SceneManager.LoadSceneAsync("Foo/Dummy", LoadSceneMode.Additive);
    //     Assert.Null("scene.invalid", loadDummyScene);
    //     // for builtin scenes, including `Assets` is invalid as a loading path
    //     loadDummyScene = SceneManager.LoadSceneAsync("Assets/Scenes/Foo/Dummy", LoadSceneMode.Additive);
    //     Assert.Null("scene.invalid", loadDummyScene);
    //
    //     testSceneBundle.assetBundle.Unload(true);
    //     prevSceneCount = SceneManager.sceneCount;
    //     // ReSharper disable once Unity.LoadSceneUnknownSceneName
    //     loadDummyScene = SceneManager.LoadSceneAsync("Assets/DummyAssets/Foo/Dummy.unity", LoadSceneMode.Additive);
    //     Assert.Null("asset_bundle_scene.unloaded", loadDummyScene);
    //     Assert.Equal("asset_bundle_scene.unloaded", 0, SceneManager.sceneCount - prevSceneCount);
    //
    //     // try load invalid asset
    //     var dummy = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "foobar"));
    //     Assert.Null("asset_bundle.invalid", dummy.assetBundle);
    //
    //     // GetSceneByName after isDone
    //     loadEmpty = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     while (!loadEmpty.isDone)
    //     {
    //         yield return null;
    //     }
    //
    //     var sceneStruct = SceneManager.GetSceneByName("Empty");
    //     Assert.Equal("scene_struct.name", "Empty", sceneStruct.name);
    //     Assert.True("scene_struct.isLoaded", sceneStruct.isLoaded);
    //
    //     prevSceneCount = SceneManager.sceneCount;
    //     var prevLoadedSceneCount = SceneManager.loadedSceneCount;
    //     // scene load additive -> scene load non-additive
    //     loadEmpty = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     var startFrame2 = Time.frameCount;
    //     loadEmpty.completed += _ => { Assert.Equal("scene.op.load_frame", 2, Time.frameCount - startFrame2); };
    //     var loadGeneral2 = SceneManager.LoadSceneAsync("General2", LoadSceneMode.Single)!;
    //     loadGeneral2.completed += _ => { Assert.Equal("scene.op.load_frame", 3, Time.frameCount - startFrame2); };
    //     LoadEmpty2 = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
    //     LoadEmpty2.completed += _ => { Assert.Equal("scene.op.load_frame", 4, Time.frameCount - startFrame2); };
    //
    //     Assert.Equal("scene.op.progress", 0.9f, loadEmpty.progress, 0.0001f);
    //     Assert.Equal("scene.op.progress", 0.9f, LoadEmpty2.progress, 0.0001f);
    //     Assert.Equal("scene.op.progress", 0.9f, loadGeneral2.progress, 0.0001f);
    //     Assert.Equal("scene.sceneCount", 3, SceneManager.sceneCount - prevSceneCount);
    //     Assert.Equal("scene.loadedSceneCount", 0, SceneManager.loadedSceneCount - prevLoadedSceneCount);
    //
    //     yield return new WaitForEndOfFrame();
    //     Assert.True("scene.op.completed_callback", loadEmptyCompleted);
    //
    //     yield return null;
    //     yield return null;
    //     // loadEmpty
    //
    //     yield return null;
    //     // General2 (this won't run)
    // }
    //
    // private readonly struct StructTest
    // {
    //     private readonly string _dummyMsg;
    //
    //     static StructTest()
    //     {
    //         // test opcode `constrained` and `callvirt` being together, this should not throw
    //         _ = new StructTest("foo").ToString();
    //     }
    //
    //     public StructTest(string dummyMsg)
    //     {
    //         _dummyMsg = dummyMsg;
    //     }
    //
    //     public override string ToString()
    //     {
    //         return _dummyMsg;
    //     }
    // }
}
