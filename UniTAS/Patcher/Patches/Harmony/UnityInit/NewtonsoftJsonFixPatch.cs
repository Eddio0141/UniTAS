using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "RedundantAssignment")]
public class NewtonsoftJsonFixPatch
{
    // stupid fucking hack, all this for getting configs to serialize with a library I just wanna use
    [HarmonyPatch("Newtonsoft.Json.Serialization.ReflectionDelegateFactory", MethodType.Getter)]
    private class GetReflectionDelegateFactory
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static MethodBase LateBoundInstanceGetter;

        private static void Prefix(ref object __result)
        {
            __result = LateBoundInstanceGetter.Invoke(null, null);
        }

        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        private static bool Prepare()
        {
            try
            {
                _ = new DynamicMethod("", typeof(void), []);
            }
            catch (Exception)
            {
                StaticLogger.LogDebug("Failed to execute dynamic method, applying hacky patch for Newtonsoft.Json");

                LateBoundInstanceGetter = AccessTools.PropertyGetter(
                    AccessTools.TypeByName("Newtonsoft.Json.Utilities.LateBoundReflectionDelegateFactory"), "Instance");
                return true;
            }

            return false;
        }
    }
}