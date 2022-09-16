using BepInEx;
using Core;
using Core.TAS.Input.Movie;
using HarmonyLib;
using System.IO;
using System.Linq;
using UnityEngine;

namespace v2021_2_14;

[BepInPlugin(Core.PluginInfo.GUID, Core.PluginInfo.NAME, Core.PluginInfo.VERSION)]
public class Plugin : BaseUnityPlugin
{
#pragma warning disable IDE0051
    private void Awake()
#pragma warning restore IDE0051
    {
        Log.SetLoggers(Logger.LogDebug, Logger.LogError, Logger.LogFatal, Logger.LogInfo, Logger.LogMessage, Logger.LogWarning);

        Log.LogInfo($"Initializing {Core.PluginInfo.NAME}");
        Log.LogInfo($"Game company name: {Application.companyName}, product name: {Application.productName}, version: {Application.version}");

        Log.LogDebug($"Unity version: {Application.unityVersion}");
        Core.PluginInfo.Init(Application.unityVersion, typeof(Plugin), typeof(UnityASyncHandler));

        var asyncHandler = new GameObject();
        asyncHandler.AddComponent<UnityASyncHandler>();
        Core.TAS.Main.AddUnityASyncHandlerID(asyncHandler.GetInstanceID());

        var harmony = new Harmony($"{Core.PluginInfo.NAME}HarmonyPatch");
        harmony.PatchAll();

        Log.LogInfo($"Plugin {Core.PluginInfo.NAME} is loaded!");
    }

    static Plugin()
    {
        var unityCoreModuleAssembly = System.AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().FullName == "UnityEngine.CoreModule").ElementAt(0);
        Core.UnityHooks.Main.Init(UnityVersion.v2021_2_14, unityCoreModuleAssembly);
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
