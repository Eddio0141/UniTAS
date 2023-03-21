using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UniTAS.Plugin.Interfaces.Patches.PatchGroups;
using UniTAS.Plugin.Utils;

namespace UniTAS.Plugin.Patches.FileSystemControlModules.FilePatchModule;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "RedundantAssignment")]
public partial class MonoIOPatchModule
{
    [MscorlibPatchGroup("3.9.9.9")]
    private class Pre4000
    {
        [HarmonyPatch]
        private class CreateDirectory
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "CreateDirectory",
                    new[] { typeof(string), MonoIOErrorType.MakeByRefType() });
            }

            private static bool Prefix(string path, ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                FileSystemManager.CreateDirectory(path);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class RemoveDirectory
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "RemoveDirectory",
                    new[] { typeof(string), MonoIOErrorType.MakeByRefType() });
            }

            private static bool Prefix(string path, ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                FileSystemManager.DeleteDirectory(path);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class GetFileSystemEntries
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "GetFileSystemEntries",
                    new[]
                    {
                        typeof(string), typeof(string), typeof(int), typeof(bool), MonoIOErrorType.MakeByRefType()
                    });
            }

            private static bool Prefix(string path, string path_with_pattern, int attrs, int mask,
                ref string[] __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                __result = FileSystemManager.GetFileSystemEntries(path, path_with_pattern, attrs, mask);

                return false;
            }
        }

        [HarmonyPatch]
        private class GetCurrentDirectory
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "GetCurrentDirectory", new[] { MonoIOErrorType.MakeByRefType() });
            }

            private static bool Prefix(ref string __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                __result = FileSystemManager.CurrentDirectory;

                return false;
            }
        }

        [HarmonyPatch]
        private class SetCurrentDirectory
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "SetCurrentDirectory",
                    new[] { typeof(string), MonoIOErrorType.MakeByRefType() });
            }

            private static bool Prefix(string path, ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                FileSystemManager.CurrentDirectory = path;
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class MoveFile
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "MoveFile",
                    new[] { typeof(string), typeof(string), MonoIOErrorType.MakeByRefType() });
            }

            private static bool Prefix(string path, string dest, ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                FileSystemManager.MoveFile(path, dest);
                __result = true;

                return false;
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
                return AccessTools.Method(MonoIOType, "CopyFile",
                    new[] { typeof(string), typeof(string), typeof(bool), MonoIOErrorType.MakeByRefType() });
            }

            private static bool Prefix(string path, string dest, bool overwrite, ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                FileSystemManager.CopyFile(path, dest, overwrite);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class DeleteFile
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "DeleteFile",
                    new[] { typeof(string), MonoIOErrorType.MakeByRefType() });
            }

            private static bool Prefix(string path, ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                FileSystemManager.DeleteFile(path);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class ReplaceFile
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "ReplaceFile",
                    new[]
                    {
                        typeof(string), typeof(string), typeof(string), typeof(bool), MonoIOErrorType.MakeByRefType()
                    });
            }

            private static bool Prefix(string sourceFileName, string destinationFileName,
                string destinationBackupFileName, bool ignoreMetadataErrors, ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                FileSystemManager.ReplaceFile(sourceFileName, destinationFileName, destinationBackupFileName,
                    ignoreMetadataErrors);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class GetFileAttributes
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "GetFileAttributes",
                    new[] { typeof(string), MonoIOErrorType.MakeByRefType() });
            }

            private static bool Prefix(string path, ref FileAttributes __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                __result = FileSystemManager.GetFileAttributes(path);

                return false;
            }
        }

        [HarmonyPatch]
        private class SetFileAttributes
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "SetFileAttributes",
                    new[] { typeof(string), typeof(FileAttributes), MonoIOErrorType.MakeByRefType() });
            }

            private static bool Prefix(string path, FileAttributes attrs, ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                FileSystemManager.SetFileAttributes(path, attrs);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class GetFileType
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "GetFileType",
                    new[] { typeof(IntPtr), MonoIOErrorType.MakeByRefType() });
            }

            private static bool Prefix(IntPtr handle, ref object __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                var fileType = (int)FileSystemManager.GetFileType(handle);
                __result = Enum.ToObject(MonoFileType, fileType);

                return false;
            }
        }

        [HarmonyPatch]
        private class GetFileStat
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "GetFileStat",
                    new[] { typeof(string), MonoIOStatType.MakeByRefType(), MonoIOErrorType.MakeByRefType() });
            }

            private static bool Prefix(string path, ref object stat, ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                var fileStat = FileSystemManager.GetFileStat(path);
                stat = AccessTools.CreateInstance(MonoIOStatType);
                var statTraverse = Traverse.Create(stat);
                statTraverse.Field("Name").SetValue(fileStat.Name);
                statTraverse.Field("Attributes").SetValue(fileStat.Attributes);
                statTraverse.Field("Length").SetValue(fileStat.Length);
                statTraverse.Field("CreationTime").SetValue(fileStat.CreationTime);
                statTraverse.Field("LastAccessTime").SetValue(fileStat.LastAccessTime);
                statTraverse.Field("LastWriteTime").SetValue(fileStat.LastWriteTime);

                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class Open
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "Open",
                    new[]
                    {
                        typeof(string), typeof(FileMode), typeof(FileAccess), typeof(FileShare), typeof(FileOptions),
                        MonoIOErrorType.MakeByRefType()
                    });
            }

            private static bool Prefix(string filename, FileMode mode, FileAccess access, FileShare share,
                FileOptions options, ref IntPtr __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                __result = FileSystemManager.Open(filename, mode, access, share, options);

                return false;
            }
        }

        [HarmonyPatch]
        private class Close
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "Close",
                    new[]
                    {
                        typeof(IntPtr), MonoIOErrorType.MakeByRefType()
                    });
            }

            private static bool Prefix(IntPtr handle, ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                FileSystemManager.Close(handle);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class Read
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "Read",
                    new[]
                    {
                        typeof(IntPtr), typeof(byte[]), typeof(int), typeof(int), MonoIOErrorType.MakeByRefType()
                    });
            }

            private static bool Prefix(IntPtr handle, byte[] dest, int dest_offset, int count, ref int __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                __result = FileSystemManager.Read(handle, dest, dest_offset, count);

                return false;
            }
        }

        [HarmonyPatch]
        private class Write
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "Write",
                    new[]
                    {
                        typeof(IntPtr), typeof(byte[]).MakeByRefType(), typeof(int), typeof(int),
                        MonoIOErrorType.MakeByRefType()
                    });
            }

            private static bool Prefix(IntPtr handle, in byte[] src, int src_offset, int count, ref int __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                __result = FileSystemManager.Write(handle, src, src_offset, count);

                return false;
            }
        }

        [HarmonyPatch]
        private class Seek
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "Seek",
                    new[] { typeof(IntPtr), typeof(long), typeof(SeekOrigin), MonoIOErrorType.MakeByRefType() });
            }

            private static bool Prefix(IntPtr handle, long offset, SeekOrigin origin, ref long __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                __result = FileSystemManager.Seek(handle, offset, origin);

                return false;
            }
        }

        [HarmonyPatch]
        private class Flush
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "Flush",
                    new[] { typeof(IntPtr), MonoIOErrorType.MakeByRefType() });
            }

            private static bool Prefix( /*IntPtr handle,*/ ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                // FileSystemManager.Flush(handle);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class GetLength
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "GetLength",
                    new[] { typeof(IntPtr), MonoIOErrorType.MakeByRefType() });
            }

            private static bool Prefix(IntPtr handle, ref long __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                __result = FileSystemManager.GetLength(handle);

                return false;
            }
        }

        [HarmonyPatch]
        private class SetLength
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "SetLength",
                    new[] { typeof(IntPtr), typeof(long), MonoIOErrorType.MakeByRefType() });
            }

            private static bool Prefix(IntPtr handle, long length, ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                FileSystemManager.SetLength(handle, length);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class SetFileTime
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "SetFileTime",
                    new[]
                    {
                        typeof(IntPtr), typeof(long), typeof(long), typeof(long), MonoIOErrorType.MakeByRefType()
                    });
            }

            private static bool Prefix(IntPtr handle, long creation_time, long last_access_time, long last_write_time,
                ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                FileSystemManager.SetFileTime(handle, creation_time, last_access_time, last_write_time);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class Lock
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "Lock",
                    new[] { typeof(IntPtr), typeof(long), typeof(long), MonoIOErrorType.MakeByRefType() });
            }

            private static bool Prefix( /*IntPtr handle, long position, long length*/)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                // TODO: Implement Lock
                // FileSystemManager.Lock(handle, position, length);

                return false;
            }
        }

        [HarmonyPatch]
        private class Unlock
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "Unlock",
                    new[] { typeof(IntPtr), typeof(long), typeof(long), MonoIOErrorType.MakeByRefType() });
            }

            private static bool Prefix( /*IntPtr handle, long position, long length*/)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                // TODO: Implement Unlock
                // FileSystemManager.Unlock(handle, position, length);

                return false;
            }
        }

        [HarmonyPatch]
        private class ConsoleOutput_get
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.PropertyGetter(MonoIOType, "ConsoleOutput");
            }

            private static bool Prefix(ref IntPtr __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                __result = FileSystemManager.ConsoleOutput;

                return false;
            }
        }

        [HarmonyPatch]
        private class ConsoleInput_get
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.PropertyGetter(MonoIOType, "ConsoleInput");
            }

            private static bool Prefix(ref IntPtr __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                __result = FileSystemManager.ConsoleInput;

                return false;
            }
        }

        [HarmonyPatch]
        private class ConsoleError_get
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.PropertyGetter(MonoIOType, "ConsoleError");
            }

            private static bool Prefix(ref IntPtr __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                __result = FileSystemManager.ConsoleError;

                return false;
            }
        }

        [HarmonyPatch]
        private class CreatePipe
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "CreatePipe",
                    new[] { typeof(IntPtr).MakeByRefType(), typeof(IntPtr).MakeByRefType() });
            }

            private static bool Prefix( /*ref IntPtr read_handle, ref IntPtr write_handle,*/ ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                // TODO Implement CreatePipe
                // FileSystemManager.CreatePipe(out read_handle, out write_handle);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class DuplicateHandle
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "DuplicateHandle",
                    new[]
                    {
                        typeof(IntPtr), typeof(IntPtr), typeof(IntPtr), typeof(IntPtr).MakeByRefType(), typeof(int),
                        typeof(int),
                        typeof(int)
                    });
            }

            private static bool
                Prefix( /*IntPtr source_process_handle, IntPtr source_handle, IntPtr target_process_handle,
                ref IntPtr target_handle, int access, int inherit, int options,*/ ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                // TODO Implement DuplicateHandle
                // FileSystemManager.DuplicateHandle(source_process_handle, source_handle, target_process_handle,
                //     out target_handle, access, inherit, options);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class VolumeSeparatorChar_get
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.PropertyGetter(MonoIOType, "VolumeSeparatorChar");
            }

            private static bool Prefix(ref char __result)
            {
                __result = FileSystemManager.VolumeSeparatorChar;

                return false;
            }
        }

        [HarmonyPatch]
        private class DirectorySeparatorChar_get
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.PropertyGetter(MonoIOType, "DirectorySeparatorChar");
            }

            private static bool Prefix(ref char __result)
            {
                __result = FileSystemManager.DirectorySeparatorChar;

                return false;
            }
        }

        [HarmonyPatch]
        private class AltDirectorySeparatorChar_get
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.PropertyGetter(MonoIOType, "AltDirectorySeparatorChar");
            }

            private static bool Prefix(ref char __result)
            {
                __result = FileSystemManager.AltDirectorySeparatorChar;

                return false;
            }
        }

        [HarmonyPatch]
        private class PathSeparator_get
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.PropertyGetter(MonoIOType, "PathSeparator");
            }

            private static bool Prefix(ref char __result)
            {
                __result = FileSystemManager.PathSeparator;

                return false;
            }
        }

        [HarmonyPatch]
        private class GetTempPath
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "GetTempPath", new[] { typeof(string).MakeByRefType() });
            }

            private static bool Prefix(ref string path, ref int __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                FileSystemManager.GetTempPath(out path);
                __result = path.Length;

                return false;
            }
        }

        [HarmonyPatch]
        private class RemapPath
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "RemapPath",
                    new[] { typeof(string), typeof(string).MakeByRefType() });
            }

            private static bool Prefix(string path, ref string newPath, ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                FileSystemManager.RemapPath(path, out newPath);
                __result = true;

                return false;
            }
        }
    }
}