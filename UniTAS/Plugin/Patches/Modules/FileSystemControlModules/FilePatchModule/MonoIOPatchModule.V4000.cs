using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UniTAS.Plugin.Patches.PatchGroups;

namespace UniTAS.Plugin.Patches.Modules.FileSystemControlModules.FilePatchModule;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "RedundantAssignment")]
public partial class MonoIOPatchModule
{
    [MscorlibPatchGroup("4.0.0.0")]
    private class V4000
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
                    new[] { typeof(char).MakePointerType(), MonoIOErrorType.MakeByRefType() });
            }

            private static unsafe bool Prefix(char* path, ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                FileSystemManager.CreateDirectory(new(path));
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
                    new[] { typeof(char).MakePointerType(), MonoIOErrorType.MakeByRefType() });
            }

            private static unsafe bool Prefix(char* path, ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                FileSystemManager.DeleteDirectory(new(path));
                __result = true;

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
                    new[] { typeof(char).MakePointerType(), MonoIOErrorType.MakeByRefType() });
            }

            private static unsafe bool Prefix(char* path, ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                FileSystemManager.CurrentDirectory = new(path);
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
                    new[]
                    {
                        typeof(char).MakePointerType(), typeof(char).MakePointerType(), MonoIOErrorType.MakeByRefType()
                    });
            }

            private static unsafe bool Prefix(char* path, char* dest, ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                FileSystemManager.MoveFile(new(path), new(dest));
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
                    new[]
                    {
                        typeof(char).MakePointerType(), typeof(char).MakePointerType(), typeof(bool),
                        MonoIOErrorType.MakeByRefType()
                    });
            }

            private static unsafe bool Prefix(char* path, char* dest, bool overwrite, ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                FileSystemManager.CopyFile(new(path), new(dest), overwrite);
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
                    new[] { typeof(char).MakePointerType(), MonoIOErrorType.MakeByRefType() });
            }

            private static unsafe bool Prefix(char* path, ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                FileSystemManager.DeleteFile(new(path));
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
                        typeof(char).MakePointerType(), typeof(char).MakePointerType(), typeof(char).MakePointerType(),
                        typeof(bool), MonoIOErrorType.MakeByRefType()
                    });
            }

            private static unsafe bool Prefix(char* sourceFileName, char* destinationFileName,
                char* destinationBackupFileName, bool ignoreMetadataErrors, ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                FileSystemManager.ReplaceFile(new(sourceFileName), new(destinationFileName),
                    new(destinationBackupFileName), ignoreMetadataErrors);
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
                    new[] { typeof(char).MakePointerType(), MonoIOErrorType.MakeByRefType() });
            }

            private static unsafe bool Prefix(char* path, ref FileAttributes __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                __result = FileSystemManager.GetFileAttributes(new(path));

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
                    new[] { typeof(char).MakePointerType(), typeof(FileAttributes), MonoIOErrorType.MakeByRefType() });
            }

            private static unsafe bool Prefix(char* path, FileAttributes attrs, ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                FileSystemManager.SetFileAttributes(new(path), attrs);
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
        private class FindFirstFile
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "FindFirstFile",
                    new[]
                    {
                        typeof(char).MakePointerType(), typeof(string).MakeByRefType(), typeof(int).MakeByRefType(),
                        typeof(int).MakeByRefType()
                    });
            }

            private static bool Prefix( /*char* pathWithPattern, ref string fileName, ref int fileAttr,
                ref int error, ref IntPtr __result*/)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                // TODO: Implement FindFirstFile

                return false;
            }
        }

        [HarmonyPatch]
        private class FindNextFile
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "FindNextFile",
                    new[]
                    {
                        typeof(IntPtr), typeof(string).MakeByRefType(), typeof(int).MakeByRefType(),
                        typeof(int).MakeByRefType()
                    });
            }

            private static bool Prefix( /*IntPtr hnd, ref string fileName, ref int fileAttr, ref int error, */
                ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                // TODO: Implement FindNextFile
                __result = false;

                return false;
            }
        }

        [HarmonyPatch]
        private class FindCloseFile
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "FindCloseFile", new[] { typeof(IntPtr) });
            }

            private static bool Prefix( /*IntPtr hnd, */ ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                // TODO: Implement FindCloseFile
                __result = false;

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
                    new[]
                    {
                        typeof(char).MakePointerType(), MonoIOStatType.MakeByRefType(), MonoIOErrorType.MakeByRefType()
                    });
            }

            private static unsafe bool Prefix(char* path, ref object stat, ref bool __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                var fileStat = FileSystemManager.GetFileStat(new(path));
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
                        typeof(char).MakePointerType(), typeof(FileMode), typeof(FileAccess), typeof(FileShare),
                        typeof(FileOptions),
                        MonoIOErrorType.MakeByRefType()
                    });
            }

            private static unsafe bool Prefix(char* filename, FileMode mode, FileAccess access, FileShare share,
                FileOptions options, ref IntPtr __result)
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                __result = FileSystemManager.Open(new(filename), mode, access, share, options);

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
                        typeof(IntPtr), typeof(byte[]), typeof(int), typeof(int), MonoIOErrorType.MakeByRefType()
                    });
            }

            private static bool Prefix(IntPtr handle, ref byte[] src, int src_offset, int count, ref int __result)
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

            private static bool Prefix( /*IntPtr handle, */ ref bool __result)
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
                    new[]
                    {
                        typeof(IntPtr).MakeByRefType(), typeof(IntPtr).MakeByRefType(), MonoIOErrorType.MakeByRefType()
                    });
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
                        typeof(int), MonoIOErrorType.MakeByRefType()
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
        private class DumpHandles
        {
            private static Exception Cleanup(MethodBase original, Exception ex)
            {
                return PatchHelper.CleanupIgnoreFail(original, ex);
            }

            private static MethodBase TargetMethod()
            {
                return AccessTools.Method(MonoIOType, "DumpHandles", Type.EmptyTypes);
            }

            private static bool Prefix()
            {
#if TRACE
                Log.Add(new StackTrace().ToString());
#endif

                FileSystemManager.DumpHandles();

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