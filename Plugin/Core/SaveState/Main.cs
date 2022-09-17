using Core.UnityHooks;
using System;
using static Core.Helper;

namespace Core.SaveState;

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

        Test = new State(sceneIndex, time, frameCount, fixedUpdateIndex);
    }

    public static void Load()
    {
        var state = Test;
        pendingLoad = true;
        pendingLoadFixedUpdateIndex = state.FixedUpdateIndex;
        pendingState = state;
    }

    public static void FixedUpdate()
    {
        if (pendingLoad && TAS.Main.FixedUpdateIndex == pendingLoadFixedUpdateIndex)
        {
            LoadOperation();
            pendingLoad = false;
        }
    }

    public static void LoadOperation()
    {
        var scene = pendingState.Scene;
        var time = pendingState.Time;
        var frameCount = pendingState.FrameCount;

        if (Scene.buildIndex(SceneManager.GetActiveScene()) != scene)
        {
            // load correct scene
            SceneManager.LoadScene(scene);
        }
        TAS.Main.Time = time;
        TAS.Main.FrameCount = frameCount;
    }
}
