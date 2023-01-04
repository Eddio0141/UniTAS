using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;
using HarmonyLib;
using UniTASPlugin.Patches.PatchGroups;

namespace UniTASPlugin.Patches.Modules.FileSystemControlModules.FilePatchModule;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public partial class PathPatchModule
{
    [MscorlibPatchGroup("4.0.0.0")]
    private class V4000
    {
        [HarmonyPatch]
        private class get_temp_path
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(typeof(Path), "get_temp_path");
            }

            private static bool Prefix(ref string __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                FileSystemManager.GetTempPath(out __result);

                return false;
            }
        }

        [HarmonyPatch]
        private class GetFullPathName
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(typeof(Path), "GetFullPathName", new[]
                {
                    typeof(string), typeof(int), typeof(StringBuilder), typeof(IntPtr).MakeByRefType()
                });
            }

            private static bool Prefix(string path, ref StringBuilder buffer, /*int numBufferChars, 
                ref IntPtr lpFilePartOrNull,*/ ref int __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                var newPath = FileSystemManager.PathToWindows(path);
                buffer = new(newPath);
                __result = newPath.Length;

                return false;
            }
        }

        // [HarmonyPatch]
        // private class Test
        // {
        //     private static MethodBase TargetMethod()
        //     {
        //         return AccessTools.Constructor(typeof(FileStream), new[]
        //         {
        //             typeof(string), typeof(FileMode), typeof(FileAccess), typeof(FileShare), typeof(int), typeof(bool),
        //             typeof(FileOptions)
        //         });
        //     }
        //
        //     private static void Prefix(string path)
        //     {
        //         Plugin.Log.LogDebug(
        //             $"path: {path}, invalid chars count: {Path.InvalidPathChars.Length}, check: {path.IndexOfAny(Path.InvalidPathChars)}");
        //     }
        // }

        // [HarmonyPatch]
        // private class Test5
        // {
        //     private static MethodBase TargetMethod()
        //     {
        //         return AccessTools.Constructor(typeof(Path), searchForStatic: true);
        //     }
        //
        //     private static void Prefix()
        //     {
        //     }
        // }

        // [HarmonyPatch]
        // private class Test2
        // {
        //     private static MethodBase TargetMethod()
        //     {
        //         return AccessTools.Method(typeof(Path), "InsecureGetFullPath");
        //     }
        //
        //     private static void Prefix(string path)
        //     {
        //         Plugin.Log.LogDebug($"before InsecureGetFullPath: {path}");
        //     }
        //
        //     private static void Postfix(string __result)
        //     {
        //         Plugin.Log.LogDebug($"after InsecureGetFullPath: {__result}");
        //     }
        // }
        //
        // [HarmonyPatch]
        // private class Test3
        // {
        //     private static MethodBase TargetMethod()
        //     {
        //         return AccessTools.Method(typeof(Path), "WindowsDriveAdjustment");
        //     }
        //
        //     private static bool Prefix(string path, ref string __result)
        //     {
        //         Plugin.Log.LogDebug(
        //             $"before WindowsDriveAdjustment: {path}, path invalid char count: {Path.InvalidPathChars.Length}");
        //         Plugin.Log.LogDebug(new StackTrace());
        //
        //         if (path.Length < 2)
        //         {
        //             __result = path.Length == 1 && (path[0] == '\\' || path[0] == '/')
        //                 ? Path.GetPathRoot(Directory.GetCurrentDirectory())
        //                 : path;
        //             Plugin.Log.LogDebug($"after WindowsDriveAdjustment: {__result}");
        //             return false;
        //         }
        //
        //         if (path[1] != ':' || !char.IsLetter(path[0]))
        //         {
        //             __result = path;
        //             Plugin.Log.LogDebug($"after WindowsDriveAdjustment: {__result}");
        //             return false;
        //         }
        //
        //         var currentDirectory = (string)typeof(Directory)
        //             .GetMethod("InsecureGetCurrentDirectory", BindingFlags.Static | BindingFlags.NonPublic)
        //             .Invoke(null, null);
        //
        //         var getFullPathName = AccessTools.Method(typeof(Path), "GetFullPathName", new[] { typeof(string) });
        //
        //         if (path.Length == 2)
        //         {
        //             MonoIOPatchModule.Log.Add("WindowsDriveAdjustment: path.Length == 2");
        //             path = currentDirectory[0] != path[0]
        //                 ? (string)getFullPathName.Invoke(null, new object[] { path })
        //                 : currentDirectory;
        //         }
        //         else if (path[2] != Path.DirectorySeparatorChar &&
        //                  path[2] != Path.AltDirectorySeparatorChar)
        //         {
        //             MonoIOPatchModule.Log.Add(
        //                 "WindowsDriveAdjustment: path[2] != Path.DirectorySeparatorChar && path[2] != Path.AltDirectorySeparatorChar");
        //             path = currentDirectory[0] != path[0]
        //                 ? (string)getFullPathName.Invoke(null, new object[] { path })
        //                 : Path.Combine(currentDirectory, path.Substring(2, path.Length - 2));
        //         }
        //
        //         __result = path;
        //         Plugin.Log.LogDebug($"after WindowsDriveAdjustment: {__result}");
        //         return false;
        //     }
        //
        //     private static void Postfix(ref string __result)
        //     {
        //         Plugin.Log.LogDebug(
        //             $"after WindowsDriveAdjustment: {__result}, path invalid char count: {Path.InvalidPathChars.Length}");
        //     }
        // }
        //
        // [HarmonyPatch]
        // private class Test4
        // {
        //     private static MethodBase TargetMethod()
        //     {
        //         return AccessTools.Method(typeof(Directory), "InsecureGetCurrentDirectory");
        //     }
        //
        //     private static void Postfix(string __result)
        //     {
        //         Plugin.Log.LogDebug($"InsecureGetCurrentDirectory returning: {__result}");
        //     }
        // }
    }
}