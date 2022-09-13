using BepInEx;
using HarmonyLib;
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

        Logger.LogInfo($"Plugin {PLUGIN_NAME} is loaded!");
    }

#pragma warning disable IDE0051
    private void Update()
#pragma warning restore IDE0051
    {
        TAS.Main.Update(Time.deltaTime);
    }
}
