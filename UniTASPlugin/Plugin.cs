using BepInEx;
using HarmonyLib;
using System.IO;
using System.Linq;
using UniTASPlugin.TAS.Input.Movie;
using UnityEngine;

namespace UniTASPlugin;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public const string PLUGIN_GUID = "____UniTASPlugin";
    public const string PLUGIN_NAME = "UniTASPlugin";
    public const string PLUGIN_VERSION = "1.0.0";

    internal static BepInEx.Logging.ManualLogSource Log;

#pragma warning disable IDE0051
    private void Awake()
#pragma warning restore IDE0051
    {
        var harmony = new Harmony("____UniTASPluginHarmonyPatch");
        harmony.PatchAll();

        Log = Logger;

        Log.LogInfo($"Company name: {Application.companyName}, product name: {Application.productName}, version: {Application.version}");
        Log.LogInfo($"Plugin {PLUGIN_NAME} is loaded!");
    }

#pragma warning disable IDE0051
    private void Update()
#pragma warning restore IDE0051
    {
        TAS.Main.Update(Time.deltaTime);

        // TODO record gameplay feature would seem interesting maybe
        // TODO simulate slowdown as user option
        // TODO GUI
        // TODO movie end notification cause memes

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

            MovieHandler.RunMovie(movie);
        }
    }

#pragma warning disable IDE0051
    private void FixedUpdate()
#pragma warning restore IDE0051
    {
        TAS.Main.FixedUpdate();
    }
}
