using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.Extensions;
using UniTASPlugin.Patches.PatchTypes;
using UniTASPlugin.Trackers.GameObjectTracker;
using Object = UnityEngine.Object;

namespace UniTASPlugin.Patches.RawPatches;

// because this is a patch that doesn't cancel the original method, we need to make sure that the original method is called
[RawPatch(1000)]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class ObjectTrackerPatch
{
    private static readonly IObjectTracker ObjectTracker = Plugin.Kernel.GetInstance<IObjectTracker>();

    [HarmonyPatch]
    private class FinalizePatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            // we target all types that can be UnityEngine.Object, excluding our project's own types
            var assemblyExclusions = new[]
            {
                "UnityEngine.*",
                "UnityEngine",
                "Unity.*",
                "System.*",
                "System",
                "netstandard",
                "mscorlib",
                "Mono.*",
                "Mono",
                "BepInEx.*",
                "BepInEx",
                "MonoMod.*",
                "0Harmony",
                "HarmonyXInterop",
                MyPluginInfo.PLUGIN_NAME,
                "StructureMap",
                "Antlr4.Runtime.Standard"
            };

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !assemblyExclusions.Any(y => x.GetName().Name.Like(y)));

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(typeof(Object)))
                    {
                        Trace.Write(
                            $"Found type {type} that is a subclass of UnityEngine.Object, patching Finalize method");
                        yield return AccessTools.Method(type, "Finalize", Type.EmptyTypes);
                    }
                }
            }
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Prefix(object __instance)
        {
            ObjectTracker.DestroyObject(__instance);
        }
    }

    [HarmonyPatch]
    private class CtorPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            // we target all types that can be UnityEngine.Object, excluding our project's own types
            var assemblyExclusions = new[]
            {
                "UnityEngine.*",
                "UnityEngine",
                "Unity.*",
                "System.*",
                "System",
                "netstandard",
                "mscorlib",
                "Mono.*",
                "Mono",
                "BepInEx.*",
                "BepInEx",
                "MonoMod.*",
                "0Harmony",
                "HarmonyXInterop",
                MyPluginInfo.PLUGIN_NAME,
                "StructureMap",
                "Antlr4.Runtime.Standard"
            };

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !assemblyExclusions.Any(y => x.GetName().Name.Like(y)));

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(typeof(Object)))
                    {
                        Trace.Write(
                            $"Found type {type} that is a subclass of UnityEngine.Object, patching ctor");
                        yield return AccessTools.Constructor(type, Type.EmptyTypes);
                    }
                }
            }
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Prefix(object __instance)
        {
            ObjectTracker.NewObject(__instance);
        }
    }

    [HarmonyPatch]
    private class DestroyPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return typeof(Object).GetMethod(nameof(Object.Destroy), AccessTools.all, null,
                new[] { typeof(Object), typeof(float) }, null);
            yield return typeof(Object).GetMethod(nameof(Object.DestroyImmediate), AccessTools.all, null,
                new[] { typeof(Object), typeof(bool) }, null);
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Postfix()
        {
            // TODO don't know if I need this
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}