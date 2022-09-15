using BepInEx;
using HarmonyLib;
using System.IO;
using UniTASPlugin.TAS.Input.Movie;
using UnityEngine;

namespace UniTASPlugin;

[BepInPlugin(Core.PluginInfo.GUID, Core.PluginInfo.NAME, Core.PluginInfo.VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static BepInEx.Logging.ManualLogSource Log;

#pragma warning disable IDE0051
    private void Awake()
#pragma warning restore IDE0051
    {
        var harmony = new Harmony("____UniTASPluginHarmonyPatch");
        harmony.PatchAll();

        Log = Logger;

        Log.LogInfo($"Company name: {Application.companyName}, product name: {Application.productName}, version: {Application.version}");
        Log.LogInfo($"Plugin {Core.PluginInfo.NAME} is loaded!");
    }

#pragma warning disable IDE0051
    private void Update()
#pragma warning restore IDE0051
    {
        GameCapture.Update();
        TAS.Main.Update(Time.deltaTime);

        // TODO remove this test
        if (!TAS.Main.Running && Input.GetKeyDown(KeyCode.K))
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
        TAS.Main.FixedUpdate();
    }
}
