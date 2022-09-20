namespace UniTASPlugin.Patches.__UnityEngine.__InputSystem;
/*
static class Helper
{
    public static Type GetKeyboard()
    {
        return AccessTools.TypeByName("UnityEngine.InputSystem.Keyboard");
    }
}

[HarmonyPatch]
class OnTextInput
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(Helper.GetKeyboard(), "OnTextInput", new Type[] { typeof(char) });
    }

    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Prefix(char character)
    {
        Plugin.Log.LogDebug($"OnTextInput({character})");
    }
}
*/