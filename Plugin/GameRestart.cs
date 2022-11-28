using System;
using UniTASPlugin.FakeGameState;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTASPlugin;

internal class GameRestart
{
    private static bool pendingFixedUpdateSoftRestart;
    private static DateTime softRestartTime;

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
    public static void SoftRestart(DateTime time)
    {
        pendingFixedUpdateSoftRestart = true;
        softRestartTime = time;
        Plugin.Instance.Logger.LogInfo("Soft restarting, pending FixedUpdate call");
    }

    private static void SoftRestartOperation()
    {
        Plugin.Instance.Logger.LogInfo("Soft restarting");

        // release mouse lock
        CursorWrap.Visible = true;
        CursorWrap.UnlockCursor();

        foreach (var obj in Object.FindObjectsOfType(typeof(MonoBehaviour)))
        {
            if (obj.GetType() == typeof(Plugin))
                continue;

            // force coroutines to stop
            (obj as MonoBehaviour).StopAllCoroutines();

            var id = obj.GetInstanceID();

            if (!GameTracker.DontDestroyOnLoadIDs.Contains(id))
                continue;
            if (GameTracker.FirstObjIDs.Contains(id))
                continue;

            // destroy all objects that are marked DontDestroyOnLoad and wasn't loaded in the first scene
            Plugin.Instance.Logger.LogDebug($"Destroying {obj.name}");
            Object.Destroy(obj);
        }

        // very game specific behavior
        /*
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
        */

        // load stored values
        foreach (var typeAndFieldAndValue in GameTracker.InitialValues)
        {
            var fieldsAndValues = typeAndFieldAndValue.Value;

            foreach (var fieldAndValue in fieldsAndValues)
            {
                var value = fieldAndValue.Value;
                var valueString = value == null ? "null" : value.ToString();
                Plugin.Instance.Logger.LogDebug($"setting field: {fieldAndValue.Key.DeclaringType.FullName}.{fieldAndValue.Key} to {valueString}");
                try
                {
                    fieldAndValue.Key.SetValue(null, value);
                }
                catch (Exception ex)
                {
                    Plugin.Instance.Logger.LogWarning($"Failed to set field: {fieldAndValue.Key.DeclaringType.FullName}.{fieldAndValue.Key} to {value} with exception: {ex}");
                }
            }
        }

        Plugin.Instance.Logger.LogDebug("finished setting fields, loading scene");
        GameTime.ResetState(softRestartTime);
        SceneHelper.LoadScene(0);

        Plugin.Instance.Logger.LogDebug("random setting state");

        RandomWrap.InitState((int)GameTime.Seed());

        Plugin.Instance.Logger.LogInfo("Finish soft restarting");
        Plugin.Instance.Logger.LogInfo($"System time: {DateTime.Now}, milliseconds: {DateTime.Now.Millisecond}");
    }
}