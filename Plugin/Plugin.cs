using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UniTASPlugin.TAS.Input.Movie;
using UnityEngine;

namespace UniTASPlugin;

[BepInPlugin(GUID, NAME, VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal const string GUID = "UniTASPlugin";
    internal const string NAME = "UniTAS";
    internal const string VERSION = "0.1.0";

    internal static BepInEx.Logging.ManualLogSource Log;

    internal static SemanticVersion UnityVersion;

    private void Awake()
    {
        Log = Logger;

        UnityVersion = Helper.GetUnityVersion();
        Log.LogInfo($"Internally found version: {UnityVersion}");
        // TODO make this version compatible, v3.5.1 doesn't have those
        //Logger.Log.LogInfo($"Game company name: {Application.companyName}, product name: {Application.productName}, version: {Application.version}");

        Harmony harmony = new($"{NAME}HarmonyPatch");
        harmony.PatchAll();

        GameObject asyncHandler = new();
        asyncHandler.AddComponent<UnityASyncHandler>();
        TAS.Main.AddUnityASyncHandlerID(asyncHandler.GetInstanceID());

        // all axis names for help
        Log.LogInfo($"All axis names: {string.Join(", ", Input.GetJoystickNames())}");

        Log.LogInfo($"Plugin {NAME} is loaded!");
    }

    private void Update()
    {
        GameCapture.Update();
        TAS.Main.Update(Time.deltaTime);

        // TODO remove this test
        if (!TAS.Main.Running && Input.GetKeyDown(KeyCode.K))
        {
            string text = File.ReadAllText("C:\\Program Files (x86)\\Steam\\steamapps\\common\\It Steals\\test.uti");
            Movie movie = new("test.uti", text, out string err, out List<string> warnings);

            if (err != "")
            {
                Log.LogError(err);
                return;
            }
            if (warnings.Count > 1)
            {
                foreach (string warn in warnings)
                {
                    Log.LogWarning(warn);
                }
            }

            TAS.Main.RunMovie(movie);
        }
        /*
        if (!TAS.Main.Running && Input.GetKeyDown(KeyCode.L))
        {
            SaveState.Main.Save();
        }
        if (!TAS.Main.Running && Input.GetKeyDown(KeyCode.O))
        {
            SaveState.Main.Load();
        }
        */
    }

    private void FixedUpdate()
    {
        TAS.Main.FixedUpdate();
    }
}