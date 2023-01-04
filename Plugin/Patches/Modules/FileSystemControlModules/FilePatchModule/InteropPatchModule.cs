using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using Microsoft.Win32.SafeHandles;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Patches.PatchGroups;
using UniTASPlugin.ReverseInvoker;
#if TRACE
using System.Diagnostics;
#endif

namespace UniTASPlugin.Patches.Modules.FileSystemControlModules.FilePatchModule;

// [MscorlibPatch]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class InteropPatchModule
{
    private static Type interopType;
    private static Type InteropType => interopType ??= AccessTools.TypeByName("Interop");

    private static Type interopSysType;

    private static Type InteropSysType =>
        interopSysType ??= InteropType.GetNestedType("Sys", AccessTools.all);

    private static Type directoryEntryType;

    private static Type DirectoryEntryType =>
        directoryEntryType ??= InteropSysType.GetNestedType("DirectoryEntry", AccessTools.all);

    private static Type fileStatusType;

    private static Type FileStatusType =>
        fileStatusType ??= InteropSysType.GetNestedType("FileStatus", AccessTools.all);

    private static Type uTimBufType;
    private static Type UTimBufType => uTimBufType ??= InteropSysType.GetNestedType("UTimBuf", AccessTools.all);

    private static Type timeValPairType;

    private static Type TimeValPairType =>
        timeValPairType ??= InteropSysType.GetNestedType("TimeValPair", AccessTools.all);

    private static readonly ReverseInvokerFactory ReverseInvokerFactory =
        Plugin.Kernel.GetInstance<ReverseInvokerFactory>();

    private static readonly IVirtualEnvironmentFactory VirtualEnvironmentFactory =
        Plugin.Kernel.GetInstance<IVirtualEnvironmentFactory>();

    [MscorlibPatchGroup(null, null, "2.1.0.0")]
    private class NetStandard21
    {
        [HarmonyPatch]
        private class GetNonCryptographicallySecureRandomBytes
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "GetNonCryptographicallySecureRandomBytes",
                    new[] { typeof(byte).MakePointerType(), typeof(int) });
            }

            private static unsafe bool Prefix(ref byte* buffer, int length)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

// #if TRACE
//                 Plugin.Log.LogDebug(new StackTrace());
// #endif

                var env = VirtualEnvironmentFactory.GetVirtualEnv();
                var randBytes = new byte[length];
                env.SystemRandom.NextBytes(randBytes);

                for (var i = 0; i < length; i++)
                {
                    buffer[i] = randBytes[i];
                }

                return false;
            }
        }

        [HarmonyPatch]
        private class OpenDir
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "OpenDir", new[] { typeof(string) });
            }

            private static void Prefix(string path, ref IntPtr __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        private class GetReadDirRBufferSize
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "GetReadDirRBufferSize", Type.EmptyTypes);
            }

            private static void Prefix(ref int __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        private class ReadDirR
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "ReadDirR",
                    new[]
                    {
                        typeof(IntPtr), typeof(byte).MakePointerType(), typeof(int), DirectoryEntryType.MakeByRefType()
                    });
            }

            private static unsafe void Prefix(IntPtr dir, byte* buffer, int bufferSize, ref object outputEntry,
                ref int __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        private class CloseDir
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "CloseDir", new[] { typeof(IntPtr) });
            }

            private static void Prefix(IntPtr dir, ref int __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        private class ReadLink
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "ReadLink",
                    new[] { typeof(string), typeof(byte[]), typeof(int) });
            }

            private static void Prefix(string path, byte[] buffer, int bufferSize, ref int __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        private class FStat
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "FStat",
                    new[] { typeof(SafeFileHandle), FileStatusType.MakeByRefType() });
            }

            private static void Prefix(SafeFileHandle fd, ref object output, ref int __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        private class Stat
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "Stat",
                    new[] { typeof(string), FileStatusType.MakeByRefType() });
            }

            private static void Prefix(string path, ref object output, ref int __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        private class LStat
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "LStat",
                    new[] { typeof(string), FileStatusType.MakeByRefType() });
            }

            private static void Prefix(string path, ref object output, ref int __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        private class Symlink
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "Symlink",
                    new[] { typeof(string), typeof(string) });
            }

            private static void Prefix(string target, string linkPath, ref int __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        private class ChMod
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "ChMod",
                    new[] { typeof(string), typeof(int) });
            }

            private static void Prefix(string path, int mode, ref int __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        private class CopyFile
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "CopyFile",
                    new[] { typeof(SafeFileHandle), typeof(SafeFileHandle) });
            }

            private static void Prefix(SafeFileHandle source, SafeFileHandle destination, ref int __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        private class GetEGid
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "GetEGid", Type.EmptyTypes);
            }

            private static void Prefix(ref uint __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        private class GetEUid
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "GetEUid", Type.EmptyTypes);
            }

            private static void Prefix(ref uint __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        // ReSharper disable once IdentifierTypo
        private class LChflags
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                // ReSharper disable once StringLiteralTypo
                return AccessTools.Method(InteropSysType, "LChflags", new[] { typeof(string), typeof(uint) });
            }

            private static void Prefix(string path, uint flags, ref int __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        // ReSharper disable once IdentifierTypo
        private class LChflagsCanSetHiddenFlag
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                // ReSharper disable once StringLiteralTypo
                return AccessTools.Method(InteropSysType, "LChflagsCanSetHiddenFlag", Type.EmptyTypes);
            }

            private static void Prefix(ref int __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        private class Link
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "Link", new[] { typeof(string), typeof(string) });
            }

            private static void Prefix(string source, string link, ref int __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        private class MkDir
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "MkDir", new[] { typeof(string), typeof(int) });
            }

            private static void Prefix(string path, int mode, ref int __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        private class Rename
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "Rename", new[] { typeof(string), typeof(string) });
            }

            private static void Prefix(string oldPath, string newPath, ref int __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        private class RmDir
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "RmDir", new[] { typeof(string) });
            }

            private static void Prefix(string path, ref int __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        private class Stat2
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "Stat",
                    new[] { typeof(byte).MakeByRefType(), FileStatusType.MakeByRefType() });
            }

            private static void Prefix(ref byte path, ref object output, ref int __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }

            private static void Postfix(object output)
            {
#if TRACE
                if (ReverseInvokerFactory.GetReverseInvoker().Invoking) return;
                var fileStatus = Traverse.Create(output);
                var flags = fileStatus.Field("Flags").GetValue<int>();
                var mode = fileStatus.Field("Mode").GetValue<int>();
                var uid = fileStatus.Field("Uid").GetValue<uint>();
                var gid = fileStatus.Field("Gid").GetValue<uint>();
                var size = fileStatus.Field("Size").GetValue<long>();
                var atime = fileStatus.Field("ATime").GetValue<long>();
                var atimeNsec = fileStatus.Field("ATimeNsec").GetValue<long>();
                var mtime = fileStatus.Field("MTime").GetValue<long>();
                var mtimeNsec = fileStatus.Field("MTimeNsec").GetValue<long>();
                var ctime = fileStatus.Field("CTime").GetValue<long>();
                var ctimeNsec = fileStatus.Field("CTimeNsec").GetValue<long>();
                var birthTime = fileStatus.Field("BirthTime").GetValue<long>();
                var birthTimeNsec = fileStatus.Field("BirthTimeNsec").GetValue<long>();
                var dev = fileStatus.Field("Dev").GetValue<long>();
                var ino = fileStatus.Field("Ino").GetValue<long>();
                var userFlags = fileStatus.Field("UserFlags").GetValue<uint>();

                Plugin.Log.LogDebug(
                    $"Stat2 postfix: flags: {flags}, mode: {mode}, uid: {uid}, gid: {gid}, size: {size}, atime: {atime}, atimeNsec: {atimeNsec}, mtime: {mtime}, mtimeNsec: {mtimeNsec}, ctime: {ctime}, ctimeNsec: {ctimeNsec}, birthTime: {birthTime}, birthTimeNsec: {birthTimeNsec}, dev: {dev}, ino: {ino}, userFlags: {userFlags}");
#endif
            }
        }

        [HarmonyPatch]
        private class LStat2
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "LStat",
                    new[] { typeof(byte).MakeByRefType(), FileStatusType.MakeByRefType() });
            }

            private static void Prefix(ref byte path, ref object output, ref int __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        private class UTime
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "UTime",
                    new[] { typeof(string), UTimBufType.MakeByRefType() });
            }

            private static void Prefix(string path, ref object time, ref int __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        private class UTimes
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "UTimes",
                    new[] { typeof(string), TimeValPairType.MakeByRefType() });
            }

            private static void Prefix(string path, ref object times, ref int __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        private class Unlink
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "Unlink", new[] { typeof(string) });
            }

            private static void Prefix(string pathname, ref int __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }

        [HarmonyPatch]
        private class DoubleToString
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(InteropSysType, "DoubleToString",
                    new[]
                    {
                        typeof(double), typeof(byte).MakePointerType(), typeof(byte).MakePointerType(), typeof(int)
                    });
            }

            private static unsafe void Prefix(double value, byte* format, byte* buffer, int bufferLength,
                ref int __result)
            {
#if TRACE
                if (!ReverseInvokerFactory.GetReverseInvoker().Invoking)
                    Plugin.Log.LogDebug(new StackTrace());
#endif
            }
        }
    }
}