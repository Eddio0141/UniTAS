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
    public static ulong FrameCount { get; private set; }
    static readonly List<string> axisNames;
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
    static bool pendingFixedUpdateSoftRestart;
    static int softRestartSeed;

    static Main()
    {
        Running = false;
        // set time to system time
        Time = System.DateTime.Now.Ticks / 10000d;
        Plugin.Log.LogInfo($"System time: {System.DateTime.Now}");
        FrameCount = 0;
        axisNames = new List<string>();
        pendingFixedUpdateSoftRestart = false;

        firstObjIDs = new List<int>();
        foreach (var obj in Object.FindObjectsOfType<MonoBehaviour>())
        {
            var id = obj.GetInstanceID();
            if (DontDestroyOnLoadIDs.Contains(id))
            {
                firstObjIDs.Add(id);
            }
        }

        var asyncHandler = new GameObject();
        asyncHandler.AddComponent<UnityASyncHandler>();
        firstObjIDs.Add(asyncHandler.GetInstanceID());

        LoadingSceneCount = 0;
    }

    public static void Update(float deltaTime)
    {
        if (Running)
        {
            Input.Main.Update();
        }

        Time += deltaTime;
        FrameCount++;
    }

    public static void FixedUpdate()
    {
        Input.Main.FixedUpdate();
        if (pendingFixedUpdateSoftRestart)
        {
            SoftRestartOperation();
            pendingFixedUpdateSoftRestart = false;
        }
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

    // BUG: on "It Steals", the game's play button breaks when you soft restart while waiting for next scene to load
    public static void SoftRestart(int seed, bool nextFixedUpdateWait)
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

        pendingFixedUpdateSoftRestart = nextFixedUpdateWait;
        softRestartSeed = seed;
        if (nextFixedUpdateWait)
        {
            Plugin.Log.LogInfo("Soft restarting, pending FixedUpdate call");
        }
        else
        {
            SoftRestartOperation();
        }
    }

    static void SoftRestartOperation()
    {
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

        Time = softRestartSeed / 1000.0;
        FrameCount = 0;
        Plugin.Log.LogInfo($"System time: {System.DateTime.Now}");

        SceneManager.LoadScene(0);

        Plugin.Log.LogInfo("Finish soft restarting");
    }
}
