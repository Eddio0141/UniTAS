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
        Log.LogInfo($"Game company name: {Application.companyName}, product name: {Application.productName}, version: {Application.version}");

        Log.LogDebug($"Unity version: {Application.unityVersion}");

        var asyncHandler = new GameObject();
        asyncHandler.AddComponent<UnityASyncHandler>();
        Core.TAS.Main.AddUnityASyncHandlerID(asyncHandler.GetInstanceID());

        var harmony = new Harmony($"{Core.PluginInfo.NAME}HarmonyPatch");
        harmony.PatchAll();

        Log.LogInfo($"Plugin {Core.PluginInfo.NAME} is loaded!");
    }

    static Plugin()
    {
        Core.PluginInfo.Init(Application.unityVersion, typeof(Plugin), typeof(UnityASyncHandler));

        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        var unityCoreModules = assemblies.Where(a => a.GetName().Name == "UnityEngine.CoreModule");

        if (unityCoreModules.Count() == 0)
        {
            Log.LogError("Found no UnityEngine.CoreModule assembly, dumping all found assemblies");
            Log.LogError(assemblies.Select(a => a.GetName().FullName));
            // TODO stop TAS tool from turning int a blackhole
        }
        else
        {
            var unityCoreModule = unityCoreModules.ElementAt(0);
            Core.UnityHooks.Main.Init(UnityVersion.v2018_4_25, unityCoreModule);
        }
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
        if (!Core.TAS.Main.Running && Input.GetKeyDown(KeyCode.L))
        {
            Core.SaveState.Main.Save();
        }
        if (!Core.TAS.Main.Running && Input.GetKeyDown(KeyCode.O))
        {
            Core.SaveState.Main.Load();
        }
    }

#pragma warning disable IDE0051
    private void FixedUpdate()
#pragma warning restore IDE0051
    {
        Core.TAS.Main.FixedUpdate();
    }
}
