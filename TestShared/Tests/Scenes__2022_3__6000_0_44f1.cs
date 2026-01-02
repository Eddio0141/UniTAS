using UnityEngine;
using UnityEngine.SceneManagement;
using System;

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
        Assert.Equal("General", SceneManager.GetActiveScene().name);
    }

    [Test]
    public void SceneStruct()
    {
        var scene = SceneManager.GetSceneByPath(emptyScene);
        Assert.False(scene.isLoaded);
        Assert.Equal(0, scene.rootCount);
        // NOTE: idk why this is so weird in editor
        for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var s = SceneManager.GetSceneByBuildIndex(i);
            Debug.Log($"thingy, {s.name}, {s.path}, {s.isSubScene}");
        }
        Assert.False(scene.isSubScene);
#if !UNITY_EDITOR
        Assert.False(scene.isSubScene);
        Assert.Equal(emptyScene, scene.path);
        Assert.NotEqual(0, scene.buildIndex);
        Assert.True(scene.IsValid());
        scene.isSubScene = true;
        Assert.True(scene.isSubScene);
        scene.isSubScene = false;
        Assert.False(scene.isSubScene);
#endif
        Assert.False(scene.isDirty);

#if !UNITY_EDITOR
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
#endif

        var sceneByName = SceneManager.GetSceneByName(scene.name);
        Assert.Equal(scene.isLoaded, sceneByName.isLoaded);
        Assert.Equal(scene.rootCount, sceneByName.rootCount);
        Assert.Equal(scene.isSubScene, sceneByName.isSubScene);
        Assert.Equal(scene.path, sceneByName.path);
        Assert.Equal(scene.buildIndex, sceneByName.buildIndex);
        Assert.Equal(scene.isDirty, sceneByName.isDirty);
        Assert.Equal(scene.IsValid(), sceneByName.IsValid());
#if !UNITY_EDITOR
        scene.isSubScene = true;
        Assert.Equal(scene.isSubScene, sceneByName.isSubScene);
        scene.isSubScene = false;
        Assert.Equal(scene.isSubScene, sceneByIdx.isSubScene);
#endif
        Assert.Equal(scene.buildIndex, sceneByName.buildIndex);
        Assert.Equal(scene.name, sceneByName.name);
    }
}
