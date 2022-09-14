using HarmonyLib;
using UnityEngine;

namespace UniTASPlugin.Patches.VirtualCursor.__UnityEngine;

#pragma warning disable IDE1006

[HarmonyPatch(typeof(Cursor), nameof(Cursor.SetCursor_Injected))]
class SetCursor_Injected
{
    static void Prefix(Texture2D texture, ref Vector2 hotspot)
    {
        TAS.Input.VirtualCursor.SetCursor(texture, hotspot);
    }
}

[HarmonyPatch(typeof(Cursor), nameof(Cursor.visible), MethodType.Setter)]
class visibleSetter
{
    static void Prefix(bool value)
    {
        // we ignore if the run is about to start since we set the visibility off
        if (TAS.Main.RunInitOrStopping)
        {
            return;
        }
        TAS.Input.VirtualCursor.Visible = value;
    }
}

[HarmonyPatch(typeof(Cursor), nameof(Cursor.visible), MethodType.Getter)]
class visibleGetter
{
    static bool Prefix(ref bool __result)
    {
        if (TAS.Main.Running)
        {
            // force the hardware cursor to be off while TAS is running
            __result = false;
            return false;
        }

        return true;
    }
}