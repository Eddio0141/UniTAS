using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GeneralTests2 : MonoBehaviour
{
    private IEnumerator Start()
    {
        Assert.Equal("scene.sceneCount", 2, SceneManager.sceneCount);
        Assert.Equal("scene.loadedSceneCount", 1, SceneManager.loadedSceneCount);

        var callTime = Time.frameCount;
        var loadEmpty2 = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
        loadEmpty2.completed += _ => { Assert.Equal("scene.op.load_frame", 2, Time.frameCount - callTime); };
        SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive);

        yield return null;
        // LoadEmpty2
        Assert.True("scene.op.isDone", GeneralTests.LoadEmpty2.isDone);

        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;

        // scene load additive -> 1f -> scene load non-additive
        var loadEmpty = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!;
        Assert.Equal("scene.op.progress", 0.9f, loadEmpty.progress, 0.0001f);
        yield return null;

        var loadGeneral3 = SceneManager.LoadSceneAsync("General3", LoadSceneMode.Single)!;
        Assert.Equal("scene.op.progress", 0.9f, loadGeneral3.progress, 0.0001f);

        var callTime2 = Time.frameCount;
        SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive)!.completed += _ =>
        {
            Assert.Equal("scene.op.load_frame", 3, Time.frameCount - callTime2);
        };

        Assert.Equal("scene.sceneCount", 7, SceneManager.sceneCount);
        Assert.Equal("scene.loadedSceneCount", 4, SceneManager.loadedSceneCount);
    }
}