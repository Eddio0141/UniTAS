using System.Collections;
using UnityEngine;

namespace UniTASPlugin.TAS;

internal class UnityASyncHandler : MonoBehaviour
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

        DontDestroyOnLoad(this);
        Instance = this;
    }

    public static void AsyncSceneLoad(AsyncOperation operation)
    {
        Instance.StartCoroutine(AsyncSceneLoadWait(operation));
    }

    static IEnumerator AsyncSceneLoadWait(AsyncOperation operation)
    {
        yield return new WaitUntil(() => operation.isDone);
        Main.LoadingSceneCount--;
    }

    public static void AsyncSceneUnload(AsyncOperation operation)
    {
        Instance.StartCoroutine(AsyncSceneUnloadWait(operation));
    }

    static IEnumerator AsyncSceneUnloadWait(AsyncOperation operation)
    {
        yield return new WaitUntil(() => operation.isDone);
        Main.UnloadingSceneCount--;
    }
}
