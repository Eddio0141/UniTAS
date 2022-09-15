using HarmonyLib;

namespace UniTASPlugin.Patches.Debug;

[HarmonyPatch(typeof(MouseLook), "Start")]
class Start
{
    static void Prefix()
    {
        if (TAS.Main.Running)
        {
            Plugin.Log.LogDebug("MouseLook.Start()");
        }
    }
}

[HarmonyPatch(typeof(MouseLook), "FixedUpdate")]
class FixedUpdate
{
    static void Prefix()
    {
        if (TAS.Main.Running)
        {
            Plugin.Log.LogDebug("MouseLook.FixedUpdate()");
        }
    }
}

[HarmonyPatch(typeof(MouseLook), "Update")]
class Update
{
    static void Prefix()
    {
        if (TAS.Main.Running)
        {
            Plugin.Log.LogDebug("MouseLook.Update()");
        }
    }
}
