using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using Microsoft.Win32.SafeHandles;
using UniTAS.Plugin.GameEnvironment;
using UniTAS.Plugin.Patches.PatchGroups;

#if TRACE
// using System.Diagnostics;
#endif

namespace UniTAS.Plugin.Patches.Modules.FileSystemControlModules.FilePatchModule;

// [MscorlibPatch]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedParameter.Local")]
public class InteropPatchModule
{
    private static readonly Type InteropType = AccessTools.TypeByName("Interop");
    private static readonly Type InteropSysType = InteropType.GetNestedType("Sys", AccessTools.all);
    private static readonly Type DirectoryEntryType = InteropSysType.GetNestedType("DirectoryEntry", AccessTools.all);
    private static readonly Type FileStatusType = InteropSysType.GetNestedType("FileStatus", AccessTools.all);
    private static readonly Type UTimBufType = InteropSysType.GetNestedType("UTimBuf", AccessTools.all);
    private static readonly Type TimeValPairType = InteropSysType.GetNestedType("TimeValPair", AccessTools.all);

    private static readonly VirtualEnvironment VirtualEnvironment =
        Plugin.Kernel.GetInstance<VirtualEnvironment>();

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
// #if TRACE
//                 Plugin.Log.LogDebug(new StackTrace());
// #endif

                var randBytes = new byte[length];
                VirtualEnvironment.SystemRandom.NextBytes(randBytes);

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

#endif
            }

            private static void Postfix(object output)
            {
#if TRACE
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

#endif
            }
        }
    }
}