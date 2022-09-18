using System.Collections;
using UniTASPlugin.TAS;
using UnityEngine;

namespace UniTASPlugin;

public class UnityASyncHandler : MonoBehaviour
{
    public static UnityASyncHandler Instance;

#pragma warning disable IDE0051
    private void Awake()
#pragma warning restore IDE0051
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        DontDestroyOnLoad(transform);
        Instance = this;
    }

    public static void AsyncSceneLoad(AsyncOperation operation)
    {
        if (operation == null)
            return;
        if (Instance == null)
        {
            Plugin.Log.LogWarning("UnityASyncHandler is null, this should not happen, skipping scene load tracker");
            Main.LoadingSceneCount = 0;
            return;
        }
        Instance.StartCoroutine(Instance.AsyncSceneLoadWait(operation));
    }

    IEnumerator AsyncSceneLoadWait(AsyncOperation operation)
    {
        // TODO does this work fine
        while (!operation.isDone)
        {
            yield return new WaitForEndOfFrame();
        }
        Main.LoadingSceneCount--;
    }

    public static void AsyncSceneUnload(AsyncOperation operation)
    {
        if (operation == null)
            return;
        if (Instance == null)
        {
            Plugin.Log.LogWarning("UnityASyncHandler is null, this should not happen, skipping scene unload tracker");
            Main.UnloadingSceneCount = 0;
            return;
        }
        Instance.StartCoroutine(Instance.AsyncSceneUnloadWait(operation));
    }

    IEnumerator AsyncSceneUnloadWait(AsyncOperation operation)
    {
        // TODO does this work fine
        while (!operation.isDone)
        {
            yield return new WaitForEndOfFrame();
        }
        Main.UnloadingSceneCount--;
    }
}