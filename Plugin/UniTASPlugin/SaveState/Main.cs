using System;

namespace UniTASPlugin.SaveState;

internal static class Main
{
    static State Test;
    static bool pendingLoad;
    static int pendingLoadFixedUpdateIndex;
    static State pendingState;

    static Main()
    {
        pendingLoad = false;
    }

    public static void Save()
    {
        var scene = SceneManager.GetActiveScene();
        var sceneIndex = Scene.buildIndex(scene);
        var time = DateTime.Now;
        var frameCount = TAS.Main.FrameCount;
        var fixedUpdateIndex = TAS.Main.FixedUpdateIndex;
        var cursorVisible = Cursor.visible;
        var cursorLockState = Cursor.lockState;
        var saveVersion = PluginInfo.UnityVersion;

        Test = new State(sceneIndex, time, frameCount, fixedUpdateIndex, cursorVisible, cursorLockState, saveVersion);
        Logger.Log.LogDebug("Saved test state");
    }

    public static void Load()
    {
        Logger.Log.LogDebug("We are loading the test state");
        var state = Test;
        pendingLoad = true;
        pendingLoadFixedUpdateIndex = state.FixedUpdateIndex;
        pendingState = state;
        Logger.Log.LogDebug($"Scene: {state.Scene}, Time: {state.Time}, FrameCount: {state.FrameCount}, FixedUpdateIndex: {state.FixedUpdateIndex}");
    }

    public static void Update()
    {
        if (pendingLoad && TAS.Main.FixedUpdateIndex == pendingLoadFixedUpdateIndex)
        {
            LoadOperation();
            pendingLoad = false;
        }
    }

    public static void LoadOperation()
    {
        Logger.Log.LogDebug("Load operation starting");
        var scene = pendingState.Scene;
        var time = pendingState.Time;
        var frameCount = pendingState.FrameCount;
        var cursorVisible = pendingState.CursorVisible;
        var cursorLockState = pendingState.CursorLockState;

        if (Scene.buildIndex(SceneManager.GetActiveScene()) != scene)
        {
            // load correct scene
            SceneManager.LoadScene(scene);
        }
        TAS.Main.Time = time;
        TAS.Main.FrameCount = frameCount;
        Cursor.visible = cursorVisible;
        Cursor.lockState = cursorLockState;

        Logger.Log.LogDebug($"Load operation finished, time: {DateTime.Now}, frameCount: {TAS.Main.FrameCount}");
    }
}
