using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GeneralTests5 : MonoBehaviour
{
    private IEnumerator Start()
    {
        // frame 1
        Assert.Equal("scene.loadedSceneCount", 1, SceneManager.loadedSceneCount);

        yield return null;
        // frame 2
        Assert.Equal("scene.loadedSceneCount", 2, SceneManager.loadedSceneCount);

        yield return null;
        // frame 3
        Assert.Equal("scene.loadedSceneCount", 3, SceneManager.loadedSceneCount);

        yield return null;
        // frame 4
        Assert.Equal("scene.loadedSceneCount", 4, SceneManager.loadedSceneCount);

        SceneManager.LoadScene("Empty", LoadSceneMode.Additive);
        var scene = SceneManager.LoadScene("Empty", new LoadSceneParameters(LoadSceneMode.Additive));
        SceneManager.LoadScene("Empty2", LoadSceneMode.Additive);

        yield return null;

        var ops = new List<AsyncOperation>
            { SceneManager.UnloadSceneAsync("Empty"), SceneManager.UnloadSceneAsync("Empty2") };

        while (!ops.TrueForAll(o => o.isDone))
        {
            yield return null;
        }

        SceneManager.SetActiveScene(scene);

        yield return null;

        Assert.Finish();
    }
}