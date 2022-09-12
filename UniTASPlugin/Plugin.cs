using BepInEx;
using HarmonyLib;

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

        new TAS();

        Log = Logger;

        Logger.LogInfo($"Plugin UniTASPlu is loaded!");
    }

    private void Update()
    {
        if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Comma))
        {
            TAS.Instance.Running = true;
        }
    }
}
