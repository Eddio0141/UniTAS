using HarmonyLib;
using System;
using UnityEngine;

namespace UniTASPlugin.SaveState;

internal static class Main
{
    static State Test;
    static bool pendingLoad = false;
    static int pendingLoadFixedUpdateIndex;
    static State pendingState;

    static object testInstance;
    static float testx;
    static float testy;
    static float testz;

    public static void Save()
    {
        // TODO only use this if unity version has it
        //var scene = SceneManager.GetActiveScene();
        //var sceneIndex = Scene.buildIndex(scene);
        DateTime time = DateTime.Now;
        ulong frameCount = TAS.Main.FrameCount;
        int fixedUpdateIndex = TAS.Main.FixedUpdateIndex;
        // TODO only save this state if unity version has it
        //var cursorVisible = Cursor.visible;
        //var cursorLockState = Cursor.lockState;
        var saveVersion = Plugin.UnityVersion;

        testInstance = UnityEngine.Object.FindObjectOfType(AccessTools.TypeByName("MouseLook"));
        var body = Traverse.Create(testInstance).Field("playerBody");
        var position = body.Property("position");
        testx = (float)position.Field("x").GetValue();
        testy = (float)position.Field("y").GetValue();
        testz = (float)position.Field("z").GetValue();

        Test = new State(/*sceneIndex,*/ time, frameCount, fixedUpdateIndex, /*cursorVisible, cursorLockState,*/ saveVersion);
        Plugin.Log.LogDebug("Saved test state");
    }

    public static void Load()
    {
        Plugin.Log.LogDebug("We are loading the test state");
        State state = Test;
        pendingLoad = true;
        pendingLoadFixedUpdateIndex = state.FixedUpdateIndex;
        pendingState = state;
        Plugin.Log.LogDebug(/*$"Scene: {state.Scene}, */$"Time: {state.Time}, FrameCount: {state.FrameCount}, FixedUpdateIndex: {state.FixedUpdateIndex}");
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
        Plugin.Log.LogDebug("Load operation starting");
        // TODO sort out depending on unity version
        //var scene = pendingState.Scene;
        DateTime time = pendingState.Time;
        ulong frameCount = pendingState.FrameCount;
        //var cursorVisible = pendingState.CursorVisible;
        //var cursorLockState = pendingState.CursorLockState;

        /*
        if (Scene.buildIndex(SceneManager.GetActiveScene()) != scene)
        {
            // load correct scene
            SceneManager.LoadScene(scene);
        }
        */
        TAS.Main.Time = time;
        TAS.Main.FrameCount = frameCount;
        //Cursor.visible = cursorVisible;
        //Cursor.lockState = cursorLockState;

        // testing
        var body = Traverse.Create(testInstance).Field("playerBody");
        var position = body.Property("position");
        position.Field("x").SetValue(testx);
        position.Field("y").SetValue(testy);
        position.Field("z").SetValue(testz);

        Plugin.Log.LogDebug($"Load operation finished, time: {DateTime.Now}, frameCount: {TAS.Main.FrameCount}");
    }
}
