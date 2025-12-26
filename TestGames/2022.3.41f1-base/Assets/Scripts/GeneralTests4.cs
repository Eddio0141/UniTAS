using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GeneralTests4 : MonoBehaviour
{
    private IEnumerator Start()
    {
        // frame 1
        Assert.Equal("scene.loadedSceneCount", 1, SceneManager.loadedSceneCount);

        yield return null;
        // frame 2
        Assert.Equal("scene.loadedSceneCount", 2, SceneManager.loadedSceneCount);

        yield return null;

        SceneManager.LoadSceneAsync("General5", LoadSceneMode.Single);
        SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive);
        SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive);
        SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive);

        Assert.Equal("scene.sceneCount", 6, SceneManager.sceneCount);
    }
}