using HarmonyLib;
using System.Threading;
using UniTASPlugin.FakeGameState;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;

namespace UniTASPlugin;

internal class GameRestart
{
    static bool pendingFixedUpdateSoftRestart = false;
    static System.DateTime softRestartTime;

    public static void FixedUpdate()
    {
        if (pendingFixedUpdateSoftRestart)
        {
            SoftRestartOperation();
            pendingFixedUpdateSoftRestart = false;
        }
    }

    /// <summary>
    /// Soft restart the game. This will not reload the game, but tries to reset the game state.
    /// Mainly used for TAS movie playback.
    /// </summary>
    /// <param name="time"></param>
    public static void SoftRestart(System.DateTime time)
    {
        if (GameTracker.LoadingSceneCount > 0)
        {
            Plugin.Log.LogInfo($"Pending soft restart, waiting on {GameTracker.LoadingSceneCount} scenes to finish loading");
            while (GameTracker.LoadingSceneCount > 0)
            {
                Thread.Sleep(1);
            }
        }
        if (GameTracker.UnloadingSceneCount > 0)
        {
            Plugin.Log.LogInfo($"Pending soft restart, waiting on {GameTracker.UnloadingSceneCount} scenes to finish loading");
            while (GameTracker.UnloadingSceneCount > 0)
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
        CursorWrap.visible = true;
        CursorWrap.UnlockCursor();

        foreach (var obj in Object.FindObjectsOfType(typeof(MonoBehaviour)))
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

            var id = obj.GetInstanceID();

            if (!GameTracker.DontDestroyOnLoadIDs.Contains(id))
                continue;

            if (GameTracker.FirstObjIDs.Contains(id))
                continue;

            // destroy all objects that are marked DontDestroyOnLoad and wasn't loaded in the first scene
            Plugin.Log.LogDebug($"Destroying {obj.name}");
            Object.Destroy(obj);
        }
        GameTime.Time = softRestartTime;
        // TODO diff unity versions
        Traverse.Create(typeof(Random)).Method("InitState", new System.Type[] { typeof(int) }).GetValue((int)TAS.Main.Seed());
        GameTime.FrameCount = 0;

        // very game specific behavior
        switch (Helper.GameName())
        {
            case "Cat Quest":
                {
                    // reset Game.instance.gameData.ai.behaviours
                    Traverse.CreateWithType("Game").Property("instance").Property("gameData").Property("ai").Field("behaviours").Method("Clear").GetValue();
                    break;
                }
            default:
                break;
        }

        // load stored values
        foreach (var typeAndFieldAndValue in GameTracker.InitialValues)
        {
            var fieldsAndValues = typeAndFieldAndValue.Value;

            foreach (var fieldAndValue in fieldsAndValues)
            {
                Plugin.Log.LogDebug($"setting field: {typeAndFieldAndValue.Key}.{fieldAndValue.Key} to {fieldAndValue.Value}");

                try
                {
                    fieldAndValue.Key.SetValue(null, fieldAndValue.Value);
                }
                catch (System.Exception ex)
                {
                    Plugin.Log.LogWarning($"Failed to set field: {typeAndFieldAndValue.Key}.{fieldAndValue.Key} to {fieldAndValue.Value} with exception: {ex}");
                }
            }
        }

        // TODO sort out depending on unity version
        Traverse sceneManager = Traverse.CreateWithType("UnityEngine.SceneManagement.SceneManager");
        sceneManager = sceneManager.Method("LoadScene", new System.Type[] { typeof(int) });
        sceneManager.GetValue(new object[] { 0 });

        Plugin.Log.LogInfo("Finish soft restarting");
        Plugin.Log.LogInfo($"System time: {System.DateTime.Now}, milliseconds: {System.DateTime.Now.Millisecond}");
    }
}