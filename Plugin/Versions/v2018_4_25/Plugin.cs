using BepInEx;
using Core;
using Core.TAS.Input.Movie;
using HarmonyLib;
using System.IO;
using System.Linq;
using UnityEngine;

namespace v2018_4_25;

[BepInPlugin(Core.PluginInfo.GUID, Core.PluginInfo.NAME, Core.PluginInfo.VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static BepInEx.Logging.ManualLogSource Log;

#pragma warning disable IDE0051
    private void Awake()
#pragma warning restore IDE0051
    {
        var asyncHandler = new GameObject();
        asyncHandler.AddComponent<UnityASyncHandler>();
        Core.TAS.Main.AddUnityASyncHandlerID(asyncHandler.GetInstanceID());

        Log = Logger;

        var harmony = new Harmony($"{Core.PluginInfo.NAME}HarmonyPatch");
        harmony.PatchAll();

        Log.LogInfo($"Plugin {Core.PluginInfo.NAME} is loaded!");
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
            var text = File.ReadAllText("C:\\Users\\eddio\\Cat Quest\\test.uti");
            var movie = new Movie("test.uti", text, out var err);

            if (err != "")
            {
                Log.LogError(err);
                return;
            }

            Core.TAS.Main.RunMovie(movie);
        }
    }

#pragma warning disable IDE0051
    private void FixedUpdate()
#pragma warning restore IDE0051
    {
        Core.TAS.Main.FixedUpdate();
    }
}