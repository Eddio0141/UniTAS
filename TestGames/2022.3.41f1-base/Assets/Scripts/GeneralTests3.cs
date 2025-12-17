using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GeneralTests3 : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;

        Assert.Equal("scene.sceneCount", 2, SceneManager.sceneCount);
        Assert.Equal("scene.loadedSceneCount", 2, SceneManager.loadedSceneCount);

        yield return null;

        // scene non-additive -> 1f -> scene load additive
        SceneManager.LoadSceneAsync("General4", LoadSceneMode.Single);
        SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Additive);

        Assert.Equal("scene.sceneCount", 4, SceneManager.sceneCount);
        Assert.Equal("scene.loadedSceneCount", 2, SceneManager.loadedSceneCount);
    }
}