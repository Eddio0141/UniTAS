using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniTASPlugin.TAS;

public static class Main
{
    static bool _running;
    public static bool Running
    {
        // TODO private set
        get => _running; set
        {
            RunInitOrStopping = true;
            if (value)
            {
                Cursor.visible = false;
            }
            else
            {
                Cursor.visible = Input.VirtualCursor.Visible;
                UnityEngine.Time.captureDeltaTime = 0f;
            }
            _running = value;
            RunInitOrStopping = false;
        }
    }
    public static bool RunInitOrStopping { get; private set; }
    public static double Time { get; private set; }
    static readonly List<string> axisNames;
    static readonly List<int> firstScenes;
    static readonly List<int> firstObjIDs;
    /// <summary>
    /// Scene loading count status. 0 means there are no scenes loading, 1 means there is one scene loading, 2 means there are two scenes loading, etc.
    /// </summary>
    public static int LoadingSceneCount { get; set; }
    /// <summary>
    /// Scene unloading count status. 0 means there are no scenes unloading, 1 means there is one scene unloading, 2 means there are two scenes unloading, etc.
    /// </summary>
    public static int UnloadingSceneCount { get; set; }
    public static List<int> DontDestroyOnLoadIDs = new();

    static Main()
    {
        // wait for TAS client to open
        // set Running depending on this

        Running = true;
        Time = 0.0;
        axisNames = new List<string>();

        firstScenes = new List<int>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            firstScenes.Add(SceneManager.GetSceneAt(i).buildIndex);
        }
        firstObjIDs = new List<int>();
        foreach (var obj in Object.FindObjectsOfType<MonoBehaviour>())
        {
            var id = obj.GetInstanceID();
            if (DontDestroyOnLoadIDs.Contains(id))
            {
                firstObjIDs.Add(id);
            }
        }

        LoadingSceneCount = 0;
    }

    public static void Update(float deltaTime)
    {
        if (Running)
        {
            Input.Main.Update();
        }

        Time += deltaTime;
    }

    public static int TimeSeed()
    {
        // TODO: work out seed calculation
        return (int)(Time * 1000.0);
    }

    public static void AxisCall(string axisName)
    {
        if (!axisNames.Contains(axisName))
        {
            axisNames.Add(axisName);

            // notify new found axis
            Plugin.Log.LogInfo($"Found new axis name: {axisName}");
        }
    }

    // BUG: on It Steals, the game's play button breaks when you soft restart while waiting for next scene to load
    public static void SoftRestart(int seed)
    {
        if (LoadingSceneCount > 0)
        {
            Plugin.Log.LogInfo($"Pending soft restart, waiting on {LoadingSceneCount} scenes to finish loading");
            while (LoadingSceneCount > 0)
            {
                Thread.Sleep(1);
            }
        }
        if (UnloadingSceneCount > 0)
        {
            Plugin.Log.LogInfo($"Pending soft restart, waiting on {UnloadingSceneCount} scenes to finish loading");
            while (UnloadingSceneCount > 0)
            {
                Thread.Sleep(1);
            }
        }
        Plugin.Log.LogInfo("Soft restarting");

        // release mouse lock
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // destroy all objects that are marked DontDestroyOnLoad and wasn't loaded in the first scene
        foreach (var obj in Object.FindObjectsOfType<MonoBehaviour>())
        {
            var id = obj.GetInstanceID();

            if (!DontDestroyOnLoadIDs.Contains(id))
                continue;

            if (firstObjIDs.Contains(id))
                continue;

            Object.Destroy(obj);
        }

        Time = seed / 1000.0;
        SceneManager.LoadScene(firstScenes[0]);
        for (int i = 1; i < firstScenes.Count; i++)
        {
            SceneManager.LoadScene(firstScenes[i], LoadSceneMode.Additive);
        }

        Plugin.Log.LogInfo("Finish soft restart");
    }
}
