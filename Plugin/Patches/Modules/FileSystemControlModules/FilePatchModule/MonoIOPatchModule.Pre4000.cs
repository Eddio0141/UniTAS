using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.Patches.PatchGroups;

namespace UniTASPlugin.Patches.Modules.FileSystemControlModules.FilePatchModule;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public partial class MonoIOPatchModule
{
    [MscorlibPatchGroup("3.9.9.9")]
    private class Pre4000
    {
        [HarmonyPatch]
        private class CreateDirectory
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "CreateDirectory", new[] { typeof(string), MonoIOErrorType });
            }

            private static bool Prefix(string path, ref bool __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                FileSystemManager.CreateDirectory(path);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class RemoveDirectory
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "RemoveDirectory", new[] { typeof(string), MonoIOErrorType });
            }

            private static bool Prefix(string path, ref bool __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                FileSystemManager.DeleteDirectory(path);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class GetFileSystemEntries
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "GetFileSystemEntries",
                    new[]
                    {
                        typeof(string), typeof(string), typeof(int), typeof(bool), MonoIOErrorType
                    });
            }

            private static bool Prefix(string path, string path_with_pattern, int attrs, int mask,
                ref string[] __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                __result = FileSystemManager.GetFileSystemEntries(path, path_with_pattern, attrs, mask);

                return false;
            }
        }

        [HarmonyPatch]
        private class GetCurrentDirectory
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "GetCurrentDirectory", new[] { MonoIOErrorType });
            }

            private static bool Prefix(ref string __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                __result = FileSystemManager.CurrentDirectory;

                return false;
            }
        }

        [HarmonyPatch]
        private class SetCurrentDirectory
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "SetCurrentDirectory", new[] { typeof(string), MonoIOErrorType });
            }

            private static bool Prefix(string path, ref bool __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                FileSystemManager.CurrentDirectory = path;
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class MoveFile
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "MoveFile",
                    new[] { typeof(string), typeof(string), MonoIOErrorType });
            }

            private static bool Prefix(string path, string dest, ref bool __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                FileSystemManager.MoveFile(path, dest);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class CopyFile
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "CopyFile",
                    new[] { typeof(string), typeof(string), typeof(bool), MonoIOErrorType });
            }

            private static bool Prefix(string path, string dest, bool overwrite, ref bool __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                FileSystemManager.CopyFile(path, dest, overwrite);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class DeleteFile
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "DeleteFile", new[] { typeof(string), MonoIOErrorType });
            }

            private static bool Prefix(string path, ref bool __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                FileSystemManager.DeleteFile(path);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class ReplaceFile
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "ReplaceFile",
                    new[] { typeof(string), typeof(string), typeof(string), typeof(bool), MonoIOErrorType });
            }

            private static bool Prefix(string sourceFileName, string destinationFileName,
                string destinationBackupFileName, bool ignoreMetadataErrors, ref bool __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                FileSystemManager.ReplaceFile(sourceFileName, destinationFileName, destinationBackupFileName,
                    ignoreMetadataErrors);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class GetFileAttributes
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "GetFileAttributes",
                    new[] { typeof(string), MonoIOErrorType });
            }

            private static bool Prefix(string path, ref FileAttributes __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                __result = FileSystemManager.GetFileAttributes(path);

                return false;
            }
        }

        [HarmonyPatch]
        private class SetFileAttributes
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "SetFileAttributes",
                    new[] { typeof(string), typeof(FileAttributes), MonoIOErrorType });
            }

            private static bool Prefix(string path, FileAttributes attrs, ref bool __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                FileSystemManager.SetFileAttributes(path, attrs);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class GetFileType
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "GetFileType", new[] { typeof(IntPtr), MonoIOErrorType });
            }

            private static bool Prefix(IntPtr handle, ref object __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                var fileType = (int)FileSystemManager.GetFileType(handle);
                __result = Enum.ToObject(MonoFileType, fileType);

                return false;
            }
        }

        [HarmonyPatch]
        private class GetFileStat
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "GetFileStat",
                    new[] { typeof(string), MonoIOStatType, MonoIOErrorType });
            }

            private static bool Prefix(string path, ref object stat, ref bool __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

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
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "Open",
                    new[]
                    {
                        typeof(string), typeof(FileMode), typeof(FileAccess), typeof(FileShare), typeof(FileOptions),
                        MonoIOErrorType
                    });
            }

            private static bool Prefix(string filename, FileMode mode, FileAccess access, FileShare share,
                FileOptions options, ref IntPtr __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                __result = FileSystemManager.Open(filename, mode, access, share, options);

                return false;
            }
        }

        [HarmonyPatch]
        private class Close
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "Close",
                    new[]
                    {
                        typeof(IntPtr), MonoIOErrorType
                    });
            }

            private static bool Prefix(IntPtr handle, ref bool __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                FileSystemManager.Close(handle);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class Read
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "Read",
                    new[] { typeof(IntPtr), typeof(byte[]), typeof(int), typeof(int), MonoIOErrorType });
            }

            private static bool Prefix(IntPtr handle, byte[] dest, int dest_offset, int count, ref int __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                __result = FileSystemManager.Read(handle, dest, dest_offset, count);

                return false;
            }
        }

        [HarmonyPatch]
        private class Write
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "Write",
                    new[] { typeof(IntPtr), typeof(byte[]), typeof(int), typeof(int), MonoIOErrorType });
            }

            private static bool Prefix(IntPtr handle, in byte[] dest, int src_offset, int count, ref int __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                __result = FileSystemManager.Write(handle, dest, src_offset, count);

                return false;
            }
        }

        [HarmonyPatch]
        private class Seek
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "Seek",
                    new[] { typeof(IntPtr), typeof(long), typeof(SeekOrigin), MonoIOErrorType });
            }

            private static bool Prefix(IntPtr handle, long offset, SeekOrigin origin, ref long __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                __result = FileSystemManager.Seek(handle, offset, origin);

                return false;
            }
        }

        [HarmonyPatch]
        private class Flush
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "Flush",
                    new[] { typeof(IntPtr), MonoIOErrorType });
            }

            private static bool Prefix( /*IntPtr handle,*/ ref bool __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                // FileSystemManager.Flush(handle);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class GetLength
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "GetLength",
                    new[] { typeof(IntPtr), MonoIOErrorType });
            }

            private static bool Prefix(IntPtr handle, ref long __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                __result = FileSystemManager.GetLength(handle);

                return false;
            }
        }

        [HarmonyPatch]
        private class SetLength
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "SetLength",
                    new[] { typeof(IntPtr), typeof(long), MonoIOErrorType });
            }

            private static bool Prefix(IntPtr handle, long length, ref bool __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                FileSystemManager.SetLength(handle, length);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class SetFileTime
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "SetFileTime",
                    new[] { typeof(IntPtr), typeof(long), typeof(long), typeof(long), MonoIOErrorType });
            }

            private static bool Prefix(IntPtr handle, long creation_time, long last_access_time, long last_write_time,
                ref bool __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                FileSystemManager.SetFileTime(handle, creation_time, last_access_time, last_write_time);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class Lock
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "Lock",
                    new[] { typeof(IntPtr), typeof(long), typeof(long), MonoIOErrorType });
            }

            private static bool Prefix( /*IntPtr handle, long position, long length*/)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                // TODO: Implement Lock
                // FileSystemManager.Lock(handle, position, length);

                return false;
            }
        }

        [HarmonyPatch]
        private class Unlock
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "Unlock",
                    new[] { typeof(IntPtr), typeof(long), typeof(long), MonoIOErrorType });
            }

            private static bool Prefix( /*IntPtr handle, long position, long length*/)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                // TODO: Implement Unlock
                // FileSystemManager.Unlock(handle, position, length);

                return false;
            }
        }

        [HarmonyPatch]
        private class ConsoleOutput_get
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.PropertyGetter(MonoIOType, "ConsoleOutput");
            }

            private static bool Prefix(ref IntPtr __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                __result = FileSystemManager.ConsoleOutput;

                return false;
            }
        }

        [HarmonyPatch]
        private class ConsoleInput_get
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.PropertyGetter(MonoIOType, "ConsoleInput");
            }

            private static bool Prefix(ref IntPtr __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                __result = FileSystemManager.ConsoleInput;

                return false;
            }
        }

        [HarmonyPatch]
        private class ConsoleError_get
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.PropertyGetter(MonoIOType, "ConsoleError");
            }

            private static bool Prefix(ref IntPtr __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                __result = FileSystemManager.ConsoleError;

                return false;
            }
        }

        [HarmonyPatch]
        private class CreatePipe
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "CreatePipe",
                    new[] { typeof(IntPtr), typeof(IntPtr) });
            }

            private static bool Prefix( /*ref IntPtr read_handle, ref IntPtr write_handle,*/ ref bool __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                // TODO Implement CreatePipe
                // FileSystemManager.CreatePipe(out read_handle, out write_handle);
                __result = true;

                return false;
            }
        }

        [HarmonyPatch]
        private class DuplicateHandle
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "DuplicateHandle",
                    new[]
                    {
                        typeof(IntPtr), typeof(IntPtr), typeof(IntPtr), typeof(IntPtr), typeof(int), typeof(int),
                        typeof(int)
                    });
            }

            private static bool
                Prefix( /*IntPtr source_process_handle, IntPtr source_handle, IntPtr target_process_handle,
                ref IntPtr target_handle, int access, int inherit, int options,*/ ref bool __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

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
            private static MethodBase TargetMethod()
            {
                return AccessTools.PropertyGetter(MonoIOType, "VolumeSeparatorChar");
            }

            private static bool Prefix(ref char __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                __result = FileSystemManager.VolumeSeparatorChar;

                return false;
            }
        }

        [HarmonyPatch]
        private class DirectorySeparatorChar_get
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.PropertyGetter(MonoIOType, "DirectorySeparatorChar");
            }

            private static bool Prefix(ref char __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                __result = FileSystemManager.DirectorySeparatorChar;

                return false;
            }
        }

        [HarmonyPatch]
        private class AltDirectorySeparatorChar_get
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.PropertyGetter(MonoIOType, "AltDirectorySeparatorChar");
            }

            private static bool Prefix(ref char __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                __result = FileSystemManager.AltDirectorySeparatorChar;

                return false;
            }
        }

        [HarmonyPatch]
        private class PathSeparator_get
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.PropertyGetter(MonoIOType, "PathSeparator");
            }

            private static bool Prefix(ref char __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                __result = FileSystemManager.PathSeparator;

                return false;
            }
        }

        [HarmonyPatch]
        private class GetTempPath
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "GetTempPath", new[] { typeof(string) });
            }

            private static bool Prefix(ref string path, ref int __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                FileSystemManager.GetTempPath(out path);
                __result = path.Length;

                return false;
            }
        }

        [HarmonyPatch]
        private class RemapPath
        {
            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "RemapPath", new[] { typeof(string), typeof(string) });
            }

            private static bool Prefix(string path, ref string newPath, ref bool __result)
            {
                var rev = ReverseInvokerFactory.GetReverseInvoker();
                if (rev.Invoking) return true;

                FileSystemManager.RemapPath(path, out newPath);
                __result = true;

                return false;
            }
        }
    }
}