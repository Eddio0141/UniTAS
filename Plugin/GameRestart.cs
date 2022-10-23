﻿using UniTASPlugin.FakeGameState;
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
        pendingFixedUpdateSoftRestart = true;
        softRestartTime = time;
        Plugin.Instance.Log.LogInfo("Soft restarting, pending FixedUpdate call");
    }

    static void SoftRestartOperation()
    {
        Plugin.Instance.Log.LogInfo("Soft restarting");

        // release mouse lock
        CursorWrap.visible = true;
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
            Plugin.Instance.Log.LogDebug($"Destroying {obj.name}");
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
                Plugin.Instance.Log.LogDebug($"setting field: {fieldAndValue.Key.DeclaringType.FullName}.{fieldAndValue.Key} to {valueString}");
                try
                {
                    fieldAndValue.Key.SetValue(null, value);
                }
                catch (System.Exception ex)
                {
                    Plugin.Instance.Log.LogWarning($"Failed to set field: {fieldAndValue.Key.DeclaringType.FullName}.{fieldAndValue.Key} to {value} with exception: {ex}");
                }
            }
        }

        Plugin.Instance.Log.LogDebug("finished setting fields, loading scene");
        GameTime.ResetState(softRestartTime);
        SceneHelper.LoadScene(0);

        Plugin.Instance.Log.LogDebug("random setting state");

        RandomWrap.InitState((int)GameTime.Seed());

        Plugin.Instance.Log.LogInfo("Finish soft restarting");
        Plugin.Instance.Log.LogInfo($"System time: {System.DateTime.Now}, milliseconds: {System.DateTime.Now.Millisecond}");
    }
}