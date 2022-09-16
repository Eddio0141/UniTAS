using Core.TAS.Input;
using Core.TAS.Input.Movie;
using Core.UnityHelpers;
using Core.UnityHelpers.Types;
using System.Collections.Generic;
using System.Threading;

namespace Core.TAS;

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
                Cursor.visible = VirtualCursor.Visible;
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
    static List<int> firstObjIDs;
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
    public static Movie CurrentMovie { get; private set; }
    public static ulong CurrentFrameNum { get; private set; }
    static int currentFramebulkIndex;
    static int currentFramebulkFrameIndex;
    static bool pendingMovieStartFixedUpdate;

    static Main()
    {
        pendingMovieStartFixedUpdate = false;
        Running = false;
        // set time to system time
        Time = System.DateTime.Now.Ticks / 10000d;
        Log.LogInfo($"System time: {System.DateTime.Now}");
        FrameCount = 0;
        axisNames = new List<string>();
        pendingFixedUpdateSoftRestart = false;

        firstObjIDs = new List<int>();
        var objs = Object.FindObjectsOfType(new Args(new object[] { typeof(UnityEngine.MonoBehaviour) }));

        foreach (var obj in objs)
        {
            var id = Object.GetInstanceID(new Args(obj, new object[] { }));
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
        UpdateMovie();
        Input.Main.Update();

        Time += deltaTime;
        FrameCount++;
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
    }

    static void UpdateMovie()
    {
        if (Running)
        {
            CurrentFrameNum++;

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

            UnityEngine.Time.captureDeltaTime = fb.Frametime;
            GameControl(fb);

            currentFramebulkFrameIndex++;
        }
    }

    static bool CheckCurrentMovieEnd()
    {
        if (currentFramebulkIndex >= CurrentMovie.Framebulks.Count)
        {
            Running = false;

            Log.LogInfo("Movie end");

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
        foreach (var (key, _) in Axis.Values)
        {
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
        foreach (var (axis, value) in fb.Axises.AxisMove)
        {
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

    public static void AxisCall(string axisName)
    {
        if (!axisNames.Contains(axisName))
        {
            axisNames.Add(axisName);

            // notify new found axis
            Log.LogInfo($"Found new axis name: {axisName}");
        }
    }
    /// <summary>
    /// Soft restart the game. This will not reload the game, but tries to reset the game state.
    /// Mainly used for TAS movie playback.
    /// </summary>
    /// <param name="seed"></param>
    public static void SoftRestart(int seed)
    {
        if (LoadingSceneCount > 0)
        {
            Log.LogInfo($"Pending soft restart, waiting on {LoadingSceneCount} scenes to finish loading");
            while (LoadingSceneCount > 0)
            {
                Thread.Sleep(1);
            }
        }
        if (UnloadingSceneCount > 0)
        {
            Log.LogInfo($"Pending soft restart, waiting on {UnloadingSceneCount} scenes to finish loading");
            while (UnloadingSceneCount > 0)
            {
                Thread.Sleep(1);
            }
        }

        pendingFixedUpdateSoftRestart = true;
        softRestartSeed = seed;
        Log.LogInfo("Soft restarting, pending FixedUpdate call");
    }

    static void SoftRestartOperation()
    {
        Log.LogInfo("Soft restarting");

        // release mouse lock
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        foreach (var obj in Object.FindObjectsOfType<MonoBehaviour>())
        {
            if (!(obj is Plugin or UnityASyncHandler))
            {
                // force coroutines to stop
                MonoBehavior.StopAllCoroutines(obj);
            }

            var id = obj.GetInstanceID();

            if (!DontDestroyOnLoadIDs.Contains(id))
                continue;

            if (firstObjIDs.Contains(id))
                continue;

            // destroy all objects that are marked DontDestroyOnLoad and wasn't loaded in the first scene
            Object.Destroy(obj);
        }

        Time = softRestartSeed / 1000.0;
        FrameCount = 0;

        SceneManager.LoadScene(0);

        Log.LogInfo("Finish soft restarting");
        Log.LogInfo($"System time: {System.DateTime.Now}");
    }

    public static void RunMovie(Movie movie)
    {
        CurrentFrameNum = 0;
        currentFramebulkIndex = 0;
        currentFramebulkFrameIndex = 1;

        CurrentMovie = movie;

        if (CurrentMovie.Framebulks.Count > 0)
        {
            var firstFb = CurrentMovie.Framebulks[0];

            Input.Main.Clear();
            UnityEngine.Time.captureDeltaTime = firstFb.Frametime;
            GameControl(firstFb);

            if (currentFramebulkFrameIndex >= firstFb.FrameCount)
            {
                currentFramebulkFrameIndex = 0;
                currentFramebulkIndex++;
            }
        }

        pendingMovieStartFixedUpdate = true;
        Log.LogInfo("Starting movie, pending FixedUpdate call");
    }

    static void RunMoviePending()
    {
        Running = true;
        SoftRestart(CurrentMovie.Seed);
        Log.LogInfo($"Movie start: {CurrentMovie}");
    }
}
