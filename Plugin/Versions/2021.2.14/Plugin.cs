using BepInEx;
using Core;
using Core.TAS.Input.Movie;
using HarmonyLib;
using System.IO;
using UnityEngine;

namespace UniTASPlugin;

[BepInPlugin(Core.PluginInfo.GUID, Core.PluginInfo.NAME, Core.PluginInfo.VERSION)]
public class Plugin : BaseUnityPlugin
{
#pragma warning disable IDE0051
    private void Awake()
#pragma warning restore IDE0051
    {
        var asyncHandler = new GameObject();
        asyncHandler.AddComponent<UnityASyncHandler>();
        Core.TAS.Main.AddUnityASyncHandlerID(asyncHandler.GetInstanceID());

        var harmony = new Harmony("____UniTASPluginHarmonyPatch");
        harmony.PatchAll();

        Log.SetLoggers(Logger.LogDebug, Logger.LogError, Logger.LogFatal, Logger.LogInfo, Logger.LogMessage, Logger.LogWarning);
        UnityHelperInit.Init(FindObjectsOfType, GetInstanceID, typeof(MonoBehaviour), typeof(KeyCode), typeof(Object));

        Log.LogInfo($"Company name: {Application.companyName}, product name: {Application.productName}, version: {Application.version}");
        Log.LogInfo($"Plugin {Core.PluginInfo.NAME} is loaded!");
    }

    static int GetInstanceID(object obj)
    {
        return (obj as Object).GetInstanceID();
    }

#pragma warning disable IDE0051
    private void Update()
#pragma warning restore IDE0051
    {
        GameCapture.Update();
        Core.TAS.Main.Update(Time.deltaTime);

        // TODO remove this test
        if (!Core.TAS.Main.Running && Input.GetKeyDown(KeyCode.K))
        {
            var text = File.ReadAllText("C:\\Program Files (x86)\\Steam\\steamapps\\common\\It Steals\\test.uti");
            var movie = new Movie("test.uti", text, out var err);

            if (err != "")
            {
                Log.LogError(err);
                return;
            }

            TAS.Main.RunMovie(movie);
        }
    }

#pragma warning disable IDE0051
    private void FixedUpdate()
#pragma warning restore IDE0051
    {
        Core.TAS.Main.FixedUpdate();
    }
}
