using HarmonyLib;
using System.Collections.Generic;
using System.Threading;
using UniTASPlugin.TAS.Input;
using UniTASPlugin.TAS.Movie;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;

namespace UniTASPlugin.TAS;

public static class Main
{
    static bool _running = false;
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
                TimeWrap.SetFrametime(0);
            }
            _running = value;
            RunInitOrStopping = false;
        }
    }
    public static bool RunInitOrStopping { get; private set; }
    public static System.DateTime Time { get; set; } = System.DateTime.MinValue;
    public static ulong FrameCount { get; set; } = 0;
    static readonly List<int> firstObjIDs = new();
    /// <summary>
    /// Scene loading count status. 0 means there are no scenes loading, 1 means there is one scene loading, 2 means there are two scenes loading, etc.
    /// </summary>
    public static int LoadingSceneCount { get; set; } = 0;
    /// <summary>
    /// Scene unloading count status. 0 means there are no scenes unloading, 1 means there is one scene unloading, 2 means there are two scenes unloading, etc.
    /// </summary>
    public static int UnloadingSceneCount { get; set; } = 0;
    public static List<int> DontDestroyOnLoadIDs = new();
    static bool pendingFixedUpdateSoftRestart = false;
    static System.DateTime softRestartTime;
    public static Movie.Movie CurrentMovie { get; private set; }
    public static ulong FrameCountMovie { get; private set; }
    static int currentFramebulkIndex;
    static int currentFramebulkFrameIndex;
    static bool pendingMovieStartFixedUpdate = false;
    public static int FixedUpdateIndex { get; private set; }

    public static void Init()
    {
        Object[] objs = Object.FindObjectsOfType(typeof(MonoBehaviour));
        foreach (Object obj in objs)
        {
            int id = obj.GetInstanceID();
            if (DontDestroyOnLoadIDs.Contains(id))
            {
                firstObjIDs.Add(id);
            }
        }
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
        if (!Running)
            return;

        FrameCountMovie++;
        if (!CheckCurrentMovieEnd())
            return;

        Framebulk fb = CurrentMovie.Framebulks[currentFramebulkIndex];
        if (currentFramebulkFrameIndex >= fb.FrameCount)
        {
            currentFramebulkIndex++;
            if (!CheckCurrentMovieEnd())
                return;

            currentFramebulkFrameIndex = 0;
            fb = CurrentMovie.Framebulks[currentFramebulkIndex];
        }

        TimeWrap.SetFrametime(fb.Frametime);
        GameControl(fb);

        currentFramebulkFrameIndex++;
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

        List<string> axisMoveSetDefault = new();
        foreach (KeyValuePair<string, float> pair in Axis.Values)
        {
            string key = pair.Key;
            if (!fb.Axises.AxisMove.ContainsKey(key))
                axisMoveSetDefault.Add(key);
        }
        foreach (string key in axisMoveSetDefault)
        {
            if (Axis.Values.ContainsKey(key))
                Axis.Values[key] = default;
            else
                Axis.Values.Add(key, default);
        }
        foreach (KeyValuePair<string, float> axisValue in fb.Axises.AxisMove)
        {
            string axis = axisValue.Key;
            float value = axisValue.Value;
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
        Traverse cursor = Traverse.CreateWithType("UnityEngine.Cursor");
        System.Type cursorLockModeType = AccessTools.TypeByName("UnityEngine.CursorLockMode");

        Traverse cursorLockState = cursor.Property("lockState");
        Traverse cursorVisible = cursor.Property("visible");

        cursorLockState.SetValue(System.Enum.Parse(cursorLockModeType, "None"));
        cursorVisible.SetValue(true);

        foreach (Object obj in Object.FindObjectsOfType(typeof(MonoBehaviour)))
        {
            if (obj.GetType() != typeof(Plugin))
            {
                // force coroutines to stop
                (obj as MonoBehaviour).StopAllCoroutines();
            }
            else
            {
                Plugin.Log.LogDebug($"Not stopping coroutines for {obj.GetType()} with Plugin type");
            }

            int id = obj.GetInstanceID();

            if (!DontDestroyOnLoadIDs.Contains(id))
                continue;

            if (firstObjIDs.Contains(id))
                continue;

            // destroy all objects that are marked DontDestroyOnLoad and wasn't loaded in the first scene
            Object.Destroy(obj);
        }

        Time = softRestartTime;
        // TODO diff unity versions
        Traverse.Create(typeof(Random)).Method("InitState", new System.Type[] { typeof(int) }).GetValue((int)Seed());
        FrameCount = 0;
        FixedUpdateIndex = 0;

        // TODO sort out depending on unity version
        Traverse sceneManager = Traverse.CreateWithType("UnityEngine.SceneManagement.SceneManager");
        sceneManager = sceneManager.Method("LoadScene", new System.Type[] { typeof(int) });
        sceneManager.GetValue(new object[] { 0 });

        Plugin.Log.LogInfo("Finish soft restarting");
        Plugin.Log.LogInfo($"System time: {System.DateTime.Now}, milliseconds: {System.DateTime.Now.Millisecond}");
    }

    public static void RunMovie(Movie.Movie movie)
    {
        FrameCountMovie = 0;
        currentFramebulkIndex = 0;
        currentFramebulkFrameIndex = 1;

        CurrentMovie = movie;

        pendingMovieStartFixedUpdate = true;
        Plugin.Log.LogInfo("Starting movie, pending FixedUpdate call");
    }

    static void RunMoviePending()
    {
        Running = true;

        if (CurrentMovie.Framebulks.Count > 0)
        {
            Framebulk firstFb = CurrentMovie.Framebulks[0];

            Input.Main.Clear();
            TimeWrap.SetFrametime(firstFb.Frametime);
            GameControl(firstFb);

            if (currentFramebulkFrameIndex >= firstFb.FrameCount)
            {
                currentFramebulkFrameIndex = 0;
                currentFramebulkIndex++;
            }
        }

        SystemInfo.DeviceType = CurrentMovie.DeviceType;
        // TODO fullscreen
        Screen.SetResolution(CurrentMovie.Width, CurrentMovie.Height, false, 60);

        SoftRestart(CurrentMovie.Time);
        Plugin.Log.LogInfo($"Movie start: {CurrentMovie}");
    }

    public static long Seed()
    {
        return Time.Ticks;
    }
}
