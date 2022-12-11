using System;
using UniTASPlugin.FixedUpdateSync;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTASPlugin;

// ReSharper disable once ClassNeverInstantiated.Global
public class GameRestart : IGameRestart
{
    private DateTime softRestartTime;

    private readonly IVirtualEnvironmentService _virtualEnvironmentService;
    private readonly ISyncFixedUpdate _syncFixedUpdate;

    public GameRestart(IVirtualEnvironmentService virtualEnvironmentService, ISyncFixedUpdate syncFixedUpdate)
    {
        _virtualEnvironmentService = virtualEnvironmentService;
        _syncFixedUpdate = syncFixedUpdate;
    }

    /// <summary>
    /// Soft restart the game. This will not reload the game, but tries to reset the game state.
    /// Mainly used for TAS movie playback.
    /// </summary>
    /// <param name="time"></param>
    public void SoftRestart(DateTime time)
    {
        _syncFixedUpdate.OnSync(SoftRestartOperation);
        softRestartTime = time;
        Plugin.Log.LogInfo("Soft restarting, pending FixedUpdate call");
    }

    private void SoftRestartOperation()
    {
        Plugin.Log.LogInfo("Soft restarting");

        // release mouse lock
        CursorWrap.Visible = true;
        CursorWrap.UnlockCursor();

        foreach (var obj in Object.FindObjectsOfType(typeof(MonoBehaviour)))
        {
            if (obj.GetType() == typeof(Plugin))
                continue;

            // force coroutines to stop
            ((MonoBehaviour)obj).StopAllCoroutines();

            var id = obj.GetInstanceID();

            if (!GameTracker.DontDestroyOnLoadIDs.Contains(id))
                continue;
            if (GameTracker.FirstObjIDs.Contains(id))
                continue;

            // destroy all objects that are marked DontDestroyOnLoad and wasn't loaded in the first scene
            Plugin.Log.LogDebug($"Destroying {obj.name}");
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
                if (fieldAndValue.Key.DeclaringType == null) continue;
                Plugin.Log.LogDebug(
                    $"setting field: {fieldAndValue.Key.DeclaringType.FullName}.{fieldAndValue.Key} to {valueString}");
                try
                {
                    fieldAndValue.Key.SetValue(null, value);
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogWarning(
                        $"Failed to set field: {fieldAndValue.Key.DeclaringType.FullName}.{fieldAndValue.Key} to {value} with exception: {ex}");
                }
            }
        }

        Plugin.Log.LogDebug("finished setting fields, loading scene");
        var env = _virtualEnvironmentService.GetVirtualEnv();
        env.GameTime.StartupTime = softRestartTime;
        SceneHelper.LoadScene(0);

        Plugin.Log.LogDebug("random setting state");

        RandomWrap.InitState((int)env.Seed);

        Plugin.Log.LogInfo("Finish soft restarting");
        Plugin.Log.LogInfo($"System time: {DateTime.Now}, milliseconds: {DateTime.Now.Millisecond}");
    }
}