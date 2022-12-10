using System;
using HarmonyLib;
using UniTASPlugin.FakeGameState;
using Object = UnityEngine.Object;

namespace UniTASPlugin.SaveState;

internal static class Main
{
    private static State Test;
    private static bool pendingLoad;
    private static int pendingLoadFixedUpdateIndex;
    private static State pendingState;

    private static object testInstance;
    private static float testx;
    private static float testy;
    private static float testz;

    public static void Save()
    {
        // TODO only use this if unity version has it
        //var scene = SceneManager.GetActiveScene();
        //var sceneIndex = Scene.buildIndex(scene);
        var time = DateTime.Now;
        var frameCount = GameTime.RenderedFrameCountOffset;
        //var fixedUpdateIndex = Plugin.Instance.FixedUpdateIndex;
        // TODO only save this state if unity version has it
        //var cursorVisible = Cursor.visible;
        //var cursorLockState = Cursor.lockState;

        testInstance = Object.FindObjectOfType(AccessTools.TypeByName("MouseLook"));
        var body = Traverse.Create(testInstance).Field("playerBody");
        var position = body.Property("position");
        testx = (float)position.Field("x").GetValue();
        testy = (float)position.Field("y").GetValue();
        testz = (float)position.Field("z").GetValue();

        //Test = new( /*sceneIndex,*/ time, frameCount, fixedUpdateIndex /*cursorVisible, cursorLockState,*/);
        Plugin.Log.LogDebug("Saved test state");
    }

    public static void Load()
    {
        Plugin.Log.LogDebug("We are loading the test state");
        var state = Test;
        pendingLoad = true;
        pendingLoadFixedUpdateIndex = state.FixedUpdateIndex;
        pendingState = state;
        Plugin.Log.LogDebug( /*$"Scene: {state.Scene}, */
            $"Time: {state.Time}, FrameCount: {state.FrameCount}, FixedUpdateIndex: {state.FixedUpdateIndex}");
    }

    public static void Update()
    {
        // if (pendingLoad && Plugin.Instance.FixedUpdateIndex == pendingLoadFixedUpdateIndex)
        // {
        //     LoadOperation();
        //     pendingLoad = false;
        // }
    }

    public static void LoadOperation()
    {
        Plugin.Log.LogDebug("Load operation starting");
        // TODO sort out depending on unity version
        //var scene = pendingState.Scene;
        //DateTime time = pendingState.Time;
        //ulong frameCount = pendingState.FrameCount;
        //FakeGameState.GameTime.SetState(time, frameCount);
        //Cursor.visible = cursorVisible;
        //Cursor.lockState = cursorLockState;

        // testing
        var body = Traverse.Create(testInstance).Field("playerBody");
        var position = body.Property("position");
        _ = position.Field("x").SetValue(testx);
        _ = position.Field("y").SetValue(testy);
        _ = position.Field("z").SetValue(testz);

        Plugin.Log.LogDebug(
            $"Load operation finished, time: {DateTime.Now}, frameCount: {GameTime.RenderedFrameCountOffset}");
    }
}