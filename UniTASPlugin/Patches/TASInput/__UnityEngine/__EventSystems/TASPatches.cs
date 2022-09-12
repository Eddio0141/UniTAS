using HarmonyLib;
using UnityEngine;

namespace UniTASPlugin.Patches.TASInput.__UnityEngine.__EventSystems;

[HarmonyPatch(typeof(UnityEngine.EventSystems.PointerEventData), "position", MethodType.Getter)]
class PointerEventDataPositionGetter
{
    static void Postfix(ref Vector2 __result)
    {
        Plugin.Log.LogDebug($"EventSystem getter called with result {__result}");
    }
}
