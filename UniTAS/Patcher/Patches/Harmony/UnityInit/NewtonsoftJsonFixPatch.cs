using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Newtonsoft.Json;
using UniTAS.Patcher.Extensions;
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
    [HarmonyPatch]
    private class GetReflectionDelegateFactory
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(
                AccessTools.GetTypesFromAssembly(typeof(JsonConvert).Assembly).FirstOrDefault(x =>
                    x.SaneFullName() == "Newtonsoft.Json.Serialization.JsonTypeReflector"),
                "ReflectionDelegateFactory");
        }

        private static MethodBase LateBoundInstanceGetter;

        private static bool Prefix(ref object __result)
        {
            __result = LateBoundInstanceGetter.Invoke(null, null);
            return false;
        }

        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        private static bool Prepare()
        {
            try
            {
                _ = new DynamicMethod("", typeof(void), []);
                return false;
            }
            catch (Exception)
            {
                LateBoundInstanceGetter = AccessTools.PropertyGetter(
                    AccessTools.GetTypesFromAssembly(typeof(JsonConvert).Assembly)
                        .FirstOrDefault(x =>
                            x.SaneFullName() == "Newtonsoft.Json.Utilities.LateBoundReflectionDelegateFactory"),
                    "Instance");
                return true;
            }
        }
    }
}