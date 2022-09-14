using BepInEx;
using HarmonyLib;
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

        new GameObject().AddComponent<TAS.UnityASyncHandler>();

        Log.LogInfo($"Plugin {PLUGIN_NAME} is loaded!");
    }

#pragma warning disable IDE0051
    private void Update()
#pragma warning restore IDE0051
    {
        TAS.Main.Update(Time.deltaTime);

        // TODO record tas would seem interesting maybe
        // TODO simulate slowdown as user option
        // TODO GUI
        // TODO movie end notification cause memes

        // TODO remove this test
        if (!TAS.Main.Running && Input.GetKeyDown(KeyCode.K))
        {
            var movie = new Movie("test", new System.Collections.Generic.List<Framebulk> {
                new Framebulk(0.001f, 2000),
                new Framebulk(0.001f, 500, new Mouse(300, 730)),
                new Framebulk(0.001f, 100, new Mouse(300, 730, true)),
                new Framebulk(0.001f, 2000, new Mouse(300, 730)),
            });

            MovieHandler.RunMovie(movie);
        }
    }
}
