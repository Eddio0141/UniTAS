using HarmonyLib;
using System.Collections.Generic;
using System.Threading;
using UniTASPlugin.TAS.Input;
using UniTASPlugin.TAS.Input.Movie;
using UnityEngine;

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
                // TODO sort out depending on unity version
                //Cursor.visible = false;
            }
            else
            {
                //Cursor.visible = VirtualCursor.Visible;
                UnityHooks.Time.captureDeltaTime = 0f;
            }
            _running = value;
            RunInitOrStopping = false;
        }
    }
    public static bool RunInitOrStopping { get; private set; }
    public static System.DateTime Time { get; set; }
    public static ulong FrameCount { get; set; }
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
    static System.DateTime softRestartTime;
    public static Movie CurrentMovie { get; private set; }
    public static ulong FrameCountMovie { get; private set; }
    static int currentFramebulkIndex;
    static int currentFramebulkFrameIndex;
    static bool pendingMovieStartFixedUpdate;
    public static int FixedUpdateIndex { get; private set; }

    static Main()
    {
        // TODO remove hooks slowly
        UnityHooks.Main.Init();

        pendingMovieStartFixedUpdate = false;
        _running = false;
        // set time to system time
        // TODO get original method result, and use that instead
        Time = System.DateTime.Now;
        Plugin.Log.LogInfo($"System time: {System.DateTime.Now}");
        FrameCount = 0;
        pendingFixedUpdateSoftRestart = false;

        firstObjIDs = new List<int>();
        var objs = Object.FindObjectsOfType(typeof(MonoBehaviour));

        foreach (var obj in objs)
        {
            var id = obj.GetInstanceID();
            if (DontDestroyOnLoadIDs.Contains(id))
            {
                firstObjIDs.Add(id);
            }
        }

        UnloadingSceneCount = 0;
        LoadingSceneCount = 0;
    }

    public static void AddUnityASyncHandlerID(int id)
    {
        firstObjIDs.Add(id);
    }

    public static void Update(float deltaTime)
    {
        SaveState.Main.Update();
        UpdateMovie();
        Input.Main.Update();

        Time += System.TimeSpan.FromSeconds(deltaTime);
        FrameCount++;
        // TODO this needs to be set at the actual unity's Update call
        FixedUpdateIndex++;
    }

    public static void FixedUpdate()
    {
        // this needs to be called before checking pending soft restart or it will cause a 1 frame desync
        if (pendingMovieStartFixedUpdate)
        {
            RunMoviePending();
            pendingMovieStartFixedUpdate = false;
        }
        if (pendingFixedUpdateSoftRestart)
        {
            SoftRestartOperation();
            pendingFixedUpdateSoftRestart = false;
        }
        // TODO this needs to be set to 0 at the actual unity's FixedUpdate call
        FixedUpdateIndex = 0;
    }

    static void UpdateMovie()
    {
        if (Running)
        {
            FrameCountMovie++;

            if (!CheckCurrentMovieEnd())
                return;

            var fb = CurrentMovie.Framebulks[currentFramebulkIndex];
            if (currentFramebulkFrameIndex >= fb.FrameCount)
            {
                currentFramebulkIndex++;
                if (!CheckCurrentMovieEnd())
                    return;

                currentFramebulkFrameIndex = 0;
                fb = CurrentMovie.Framebulks[currentFramebulkIndex];
            }

            UnityHooks.Time.captureDeltaTime = fb.Frametime;
            GameControl(fb);

            currentFramebulkFrameIndex++;
        }
    }

    static bool CheckCurrentMovieEnd()
    {
        if (currentFramebulkIndex >= CurrentMovie.Framebulks.Count)
        {
            Running = false;

            Plugin.Log.LogInfo("Movie end");

            return false;
        }

        return true;
    }

    static void GameControl(Framebulk fb)
    {
        Input.Mouse.Position = new Vector2(fb.Mouse.X, fb.Mouse.Y);
        Input.Mouse.LeftClick = fb.Mouse.Left;
        Input.Mouse.RightClick = fb.Mouse.Right;
        Input.Mouse.MiddleClick = fb.Mouse.Middle;

        var axisMoveSetDefault = new List<string>();
        foreach (var pair in Axis.Values)
        {
            var key = pair.Key;
            if (!fb.Axises.AxisMove.ContainsKey(key))
                axisMoveSetDefault.Add(key);
        }
        foreach (var key in axisMoveSetDefault)
        {
            if (Axis.Values.ContainsKey(key))
                Axis.Values[key] = default;
            else
                Axis.Values.Add(key, default);
        }
        foreach (var axisValue in fb.Axises.AxisMove)
        {
            var axis = axisValue.Key;
            var value = axisValue.Value;
            if (Axis.Values.ContainsKey(axis))
            {
                Axis.Values[axis] = value;
            }
            else
            {
                Axis.Values.Add(axis, value);
            }
        }
    }

    /// <summary>
    /// Soft restart the game. This will not reload the game, but tries to reset the game state.
    /// Mainly used for TAS movie playback.
    /// </summary>
    /// <param name="time"></param>
    public static void SoftRestart(System.DateTime time)
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

        pendingFixedUpdateSoftRestart = true;
        softRestartTime = time;
        Plugin.Log.LogInfo("Soft restarting, pending FixedUpdate call");
    }

    static void SoftRestartOperation()
    {
        Plugin.Log.LogInfo("Soft restarting");

        // release mouse lock
        // TODO sort out depending on unity version
        var cursor = Traverse.CreateWithType("UnityEngine.Cursor");
        var cursorLockModeType = AccessTools.TypeByName("UnityEngine.CursorLockMode");

        var cursorLockState = cursor.Property("lockState");
        var cursorVisible = cursor.Property("visible");

        cursorLockState.SetValue(System.Enum.Parse(cursorLockModeType, "None"));
        cursorVisible.SetValue(true);

        foreach (var obj in Object.FindObjectsOfType(typeof(MonoBehaviour)))
        {
            if (!(obj.GetType() == typeof(Plugin) || obj.GetType() == typeof(UnityASyncHandler)))
            {
                // force coroutines to stop
                (obj as MonoBehaviour).StopAllCoroutines();
            }
            else
            {
                Plugin.Log.LogDebug($"Not stopping coroutines for {obj.GetType()} with Plugin type and UnityASyncHandlerType");
            }

            var id = obj.GetInstanceID();

            if (!DontDestroyOnLoadIDs.Contains(id))
                continue;

            if (firstObjIDs.Contains(id))
                continue;

            // destroy all objects that are marked DontDestroyOnLoad and wasn't loaded in the first scene
            Object.Destroy(obj);
        }

        Time = softRestartTime;
        FrameCount = 0;
        FixedUpdateIndex = 0;

        // TODO sort out depending on unity version
        var sceneManager = Traverse.CreateWithType("UnityEngine.SceneManagement.SceneManager");
        sceneManager = sceneManager.Method("LoadScene", new System.Type[] { typeof(int) });
        sceneManager.GetValue(new object[] { 0 });

        Plugin.Log.LogInfo("Finish soft restarting");
        Plugin.Log.LogInfo($"System time: {System.DateTime.Now}, milliseconds: {System.DateTime.Now.Millisecond}");
    }

    public static void RunMovie(Movie movie)
    {
        FrameCountMovie = 0;
        currentFramebulkIndex = 0;
        currentFramebulkFrameIndex = 1;

        CurrentMovie = movie;

        if (CurrentMovie.Framebulks.Count > 0)
        {
            var firstFb = CurrentMovie.Framebulks[0];

            Input.Main.Clear();
            // TODO
            UnityHooks.Time.captureDeltaTime = firstFb.Frametime;
            GameControl(firstFb);

            if (currentFramebulkFrameIndex >= firstFb.FrameCount)
            {
                currentFramebulkFrameIndex = 0;
                currentFramebulkIndex++;
            }
        }

        pendingMovieStartFixedUpdate = true;
        Plugin.Log.LogInfo("Starting movie, pending FixedUpdate call");
    }

    static void RunMoviePending()
    {
        Running = true;
        SoftRestart(CurrentMovie.Time);
        Plugin.Log.LogInfo($"Movie start: {CurrentMovie}");
    }
}
