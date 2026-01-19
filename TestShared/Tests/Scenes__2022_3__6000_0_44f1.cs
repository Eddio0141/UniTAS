using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public class Scenes__2022_3__6000_0_44f1 : MonoBehaviour
{
    [TestInjectScene] public string emptyScene;

    [Test(InitTestTiming.Awake)]
    public void UnloadInvalid()
    {
        Assert.Throws(new ArgumentException("Scene to unload is invalid"), () => SceneManager.UnloadSceneAsync(emptyScene));
    }

    [Test(InitTestTiming.Awake)]
    public void InitialScene()
    {
        Assert.Equal("general", SceneManager.GetActiveScene().name);
    }

    [Test]
    public IEnumerator<TestYield> SceneStruct()
    {
        // the scene isn't loaded yet
        var scene = SceneManager.GetSceneByPath(emptyScene);
        Assert.True(string.IsNullOrEmpty(scene.name));
        Assert.True(string.IsNullOrEmpty(scene.path));

        SceneManager.LoadScene(emptyScene);

        yield return new UnityYield(null);

        scene = SceneManager.GetSceneByPath(emptyScene);
        Assert.True(scene.isLoaded);
        Assert.Equal(0, scene.rootCount);
        Assert.False(scene.isSubScene);
        Assert.Equal(emptyScene, scene.path);
        Assert.NotEqual(0, scene.buildIndex);
        Assert.True(scene.IsValid());
        scene.isSubScene = true;
        Assert.True(scene.isSubScene);
        scene.isSubScene = false;
        Assert.False(scene.isSubScene);
        Assert.False(scene.isDirty);

        var sceneByIdx = SceneManager.GetSceneByBuildIndex(scene.buildIndex);
        Assert.Equal(scene.isLoaded, sceneByIdx.isLoaded);
        Assert.Equal(scene.rootCount, sceneByIdx.rootCount);
        Assert.Equal(scene.isSubScene, sceneByIdx.isSubScene);
        Assert.Equal(scene.path, sceneByIdx.path);
        Assert.Equal(scene.buildIndex, sceneByIdx.buildIndex);
        Assert.Equal(scene.isDirty, sceneByIdx.isDirty);
        Assert.Equal(scene.IsValid(), sceneByIdx.IsValid());
        scene.isSubScene = true;
        Assert.Equal(scene.isSubScene, sceneByIdx.isSubScene);
        scene.isSubScene = false;
        Assert.Equal(scene.isSubScene, sceneByIdx.isSubScene);
        Assert.Equal(scene.buildIndex, sceneByIdx.buildIndex);
        Assert.Equal(scene.name, sceneByIdx.name);

        var sceneByName = SceneManager.GetSceneByName(scene.name);
        Assert.Equal(scene.isLoaded, sceneByName.isLoaded);
        Assert.Equal(scene.rootCount, sceneByName.rootCount);
        Assert.Equal(scene.isSubScene, sceneByName.isSubScene);
        Assert.Equal(scene.path, sceneByName.path);
        Assert.Equal(scene.buildIndex, sceneByName.buildIndex);
        Assert.Equal(scene.isDirty, sceneByName.isDirty);
        Assert.Equal(scene.IsValid(), sceneByName.IsValid());
        scene.isSubScene = true;
        Assert.Equal(scene.isSubScene, sceneByName.isSubScene);
        scene.isSubScene = false;
        Assert.Equal(scene.isSubScene, sceneByIdx.isSubScene);
        Assert.Equal(scene.buildIndex, sceneByName.buildIndex);
        Assert.Equal(scene.name, sceneByName.name);

        Assert.Throws(new InvalidOperationException($"Setting a name on a saved scene is not allowed (the filename is used as name). Scene: '{emptyScene}'"), () => scene.name = "foo");
    }

    [TestInjectScene] public string loadAsyncStallAdditive1fScene;

    [Test]
    public IEnumerator<TestYield> LoadAsyncStallAdditive1f()
    {
        var startFrame = Time.frameCount;
        var op = SceneManager.LoadSceneAsync(loadAsyncStallAdditive1fScene, LoadSceneMode.Additive)!;
        var scene = SceneManager.GetSceneByPath(loadAsyncStallAdditive1fScene);
        Assert.False(scene.isSubScene);
        scene.isSubScene = true;
        Assert.Equal(scene.path, loadAsyncStallAdditive1fScene);

        op.completed += _ =>
        {
            Assert.Equal(2, SceneManager.sceneCount);
            Assert.Equal(2, SceneManager.loadedSceneCount);
            Assert.Equal(2, Time.frameCount - startFrame);

            var actualScene = SceneManager.GetSceneByPath(loadAsyncStallAdditive1fScene);
            Assert.True(scene == actualScene);
            Assert.False(scene != actualScene);
            Assert.True(scene.Equals(actualScene));
            Assert.True(scene.isLoaded);
            Assert.Equal(0, scene.rootCount);
            Assert.True(scene.isSubScene);
            Assert.Equal(loadAsyncStallAdditive1fScene, scene.path);
            Assert.False(scene.isDirty);
            Assert.True(scene.IsValid());
            Assert.NotEqual(0, scene.handle);
            Assert.NotEqual(0, scene.GetHashCode());
        };

        Assert.Equal(op.priority, 0);
#if !UNITY_EDITOR
        Assert.Equal(op.progress, 0.9f);
#endif
        Assert.False(op.isDone);

        op.allowSceneActivation = false;

        Assert.Throws(new ArgumentException($"SceneManager.SetActiveScene failed; scene '{scene.name}' is not loaded and therefore cannot be set active"), () => SceneManager.SetActiveScene(scene));

        yield return new UnityYield(null);

        Assert.Equal(2, SceneManager.sceneCount);
        Assert.Equal(1, SceneManager.loadedSceneCount);

        Assert.False(op.isDone);
        op.allowSceneActivation = true;
        Assert.False(op.isDone);

        yield return new UnityYield(null);

        Assert.True(op.isDone);
        Assert.Equal(1f, op.progress);
    }
}
