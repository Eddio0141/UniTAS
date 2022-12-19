using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using HarmonyLib;
using Microsoft.Win32.SafeHandles;
using UniTASPlugin.FakeGameState.GameFileSystem;
using UniTASPlugin.ReverseInvoker;
using DirOrig = System.IO.Directory;
using FileStreamOrig = System.IO.FileStream;
using PathOrig = System.IO.Path;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable StringLiteralTypo

namespace UniTASPlugin.Patches.System.IO;

[HarmonyPatch]
internal static class FileStream
{
    private static class Helper
    {
        public static Traverse WriteInternalTraverse(FileStreamOrig instance)
        {
            return Traverse.Create(instance)
                .Method("WriteInternal", new[] { typeof(byte[]), typeof(int), typeof(int) });
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), MethodType.Constructor, typeof(string), typeof(FileMode), typeof(FileAccess),
        typeof(FileShare), typeof(int), typeof(bool), typeof(FileOptions))]
    private class Ctor__string__FileMode__FileAccess__FileShare__int__bool__FileOptions
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref FileStreamOrig __instance, ref string path, FileMode mode, FileAccess access,
            ref FileShare share, ref int bufferSize, bool anonymous, FileOptions options)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            var instanceTraverse = Traverse.Create(__instance);
            _ = instanceTraverse.Field("name").SetValue("[Unknown]");
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path.Length == 0)
            {
                throw new ArgumentException("Path is empty");
            }

            _ = instanceTraverse.Field("anonymous").SetValue(anonymous);
            share &= ~FileShare.Inheritable;
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "Positive number required.");
            }

            if (mode is < FileMode.CreateNew or > FileMode.Append)
            {
                if (anonymous)
                {
                    throw new ArgumentException("Enum value was out of legal range.", nameof(mode));
                }

                throw new ArgumentOutOfRangeException(nameof(mode), "Enum value was out of legal range.");
            }

            if (access is < FileAccess.Read or > FileAccess.ReadWrite)
            {
                throw new ArgumentOutOfRangeException(nameof(access), "Enum value was out of legal range.");
            }

            if (share is < FileShare.None or > (FileShare.Read | FileShare.Write | FileShare.Delete))
            {
                throw new ArgumentOutOfRangeException(nameof(share), "Enum value was out of legal range.");
            }
#pragma warning disable CS0618 // Type or member is obsolete
            if (path.IndexOfAny(PathOrig.InvalidPathChars) != -1)
            {
                throw new ArgumentException("Name has invalid chars");
            }
#pragma warning restore CS0618 // Type or member is obsolete
            var pathTraverse = Traverse.Create(typeof(PathOrig));
            path = pathTraverse.Method("InsecureGetFullPath", new[] { typeof(string) }).GetValue<string>(path);
            if (DirOrig.Exists(path))
            {
                var getSecureFileName = Traverse.Create(typeof(FileStreamOrig))
                    .Method("GetSecureFileName", new[] { typeof(string) });
                throw new UnauthorizedAccessException(
                    $"Access to the path '{getSecureFileName.GetValue(path, false)}' is denied.");
            }

            if (mode == FileMode.Append && (access & FileAccess.Read) == FileAccess.Read)
            {
                throw new ArgumentException("Append access can be requested only in write-only mode.");
            }

            if ((access & FileAccess.Write) == 0 && mode != FileMode.Open && mode != FileMode.OpenOrCreate)
            {
                throw new ArgumentException($"Combining FileMode: {access} with FileAccess: {mode} is invalid.");
            }

            var directoryName = PathOrig.GetDirectoryName(path);
            // ReSharper disable once PossibleNullReferenceException
            if (directoryName.Length > 0 && !DirOrig.Exists(PathOrig.GetFullPath(directoryName)))
            {
                var arg = anonymous ? directoryName : PathOrig.GetFullPath(path);
                throw new DirectoryNotFoundException($"Could not find a part of the path \"{arg}\".");
            }

            //if (!anonymous)
            //{
            _ = instanceTraverse.Field("name").SetValue(path);
            //}
            FileSystem.OsHelpers.OpenFile(path, mode, access, share, options);
            _ = instanceTraverse.Field("access").SetValue(access);
            _ = instanceTraverse.Field("owner").SetValue(true);
            _ = instanceTraverse.Field("canseek").SetValue(true);
            _ = instanceTraverse.Field("async").SetValue((options & FileOptions.Asynchronous) > FileOptions.None);
            if (access == FileAccess.Read && bufferSize == 4096)
            {
                var length = __instance.Length;
                if (bufferSize > length)
                {
                    bufferSize = (int)((length < 1000L) ? 1000L : length);
                }
            }

            _ = instanceTraverse.Method("InitBuffer", new[] { typeof(int), typeof(bool) }).GetValue(bufferSize, false);
            if (mode == FileMode.Append)
            {
                _ = __instance.Seek(0L, SeekOrigin.End);
                _ = instanceTraverse.Field("append_startpos").SetValue(__instance.Position);
                return false;
            }

            _ = instanceTraverse.Field("append_startpos").SetValue(0L);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.Seek))]
    private class Seek
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref long __result, ref FileStreamOrig __instance, long offset, SeekOrigin origin)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            if (!__instance.CanSeek)
            {
                throw new NotSupportedException("The stream does not support seeking");
            }

            var num = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => __instance.Position + offset,
                SeekOrigin.End => __instance.Length + offset,
                _ => throw new ArgumentException("Invalid SeekOrigin", nameof(origin))
            };
            if (num < 0L)
            {
                throw new IOException("Attempted to Seek before the beginning of the stream");
            }

            var instanceTraverse = Traverse.Create(__instance);
            if (num < instanceTraverse.Field("append_startpos").GetValue<long>())
            {
                throw new IOException("Can't seek back over pre-existing data in append mode");
            }

            _ = instanceTraverse.Method("FlushBuffer").GetValue();
            var seekResult = FileSystem.OsHelpers.Seek(instanceTraverse.Field("name").GetValue<string>(), num,
                SeekOrigin.Begin);
            _ = instanceTraverse.Field("buf_start").SetValue(seekResult);
            __result = seekResult;
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.Length), MethodType.Getter)]
    private class get_Length
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref long __result, ref FileStreamOrig __instance)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            if (!__instance.CanSeek)
            {
                throw new NotSupportedException("The stream does not support seeking");
            }

            var instanceTraverse = Traverse.Create(__instance);
            _ = instanceTraverse.Method("FlushBufferIfDirty").GetValue();
            __result = FileSystem.OsHelpers.Length(instanceTraverse.Field("name").GetValue<string>());
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.Position), MethodType.Getter)]
    private class get_Position
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref long __result, ref FileStreamOrig __instance)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            if (!__instance.CanSeek)
            {
                throw new NotSupportedException("The stream does not support seeking");
            }

            var instanceTraverse = Traverse.Create(__instance);
            if (!instanceTraverse.Field("isExposed").GetValue<bool>())
            {
                __result = instanceTraverse.Field("buf_start").GetValue<long>() +
                           instanceTraverse.Field("buf_offset").GetValue<int>();
                return false;
            }

            __result = FileSystem.OsHelpers.Seek(instanceTraverse.Field("name").GetValue<string>(), 0,
                SeekOrigin.Current);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.Write))]
    private class Write
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref FileStreamOrig __instance, byte[] array, int offset, int count)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "< 0");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "< 0");
            }

            if (offset > array.Length - count)
            {
                throw new ArgumentException("Reading would overrun buffer");
            }

            if (!__instance.CanWrite)
            {
                throw new NotSupportedException("Stream does not support writing");
            }

            var instanceTraverse = Traverse.Create(__instance);
            if (instanceTraverse.Field("async").GetValue<bool>())
            {
                var asyncResult = __instance.BeginWrite(array, offset, count, null, null);
                __instance.EndWrite(asyncResult);
                return false;
            }

            _ = Helper.WriteInternalTraverse(__instance).GetValue(array, offset, count);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.Dispose))]
    private class Dispose
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref FileStreamOrig __instance, bool disposing)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            Exception ex = null;
            var instanceTraverse = Traverse.Create(__instance);
            var name = instanceTraverse.Field("name").GetValue<string>();
            FileSystem.OsHelpers.Close(name);
            _ = instanceTraverse.Field("canseek").SetValue(false);
            _ = instanceTraverse.Field("access").SetValue((FileAccess)0);
            var bufTraverse = instanceTraverse.Field("buf");
            var buf = bufTraverse.GetValue<byte[]>();
            if (disposing && buf != null)
            {
                var fileStreamTraverse = Traverse.Create(typeof(FileStreamOrig));
                var buf_recycleTraverse = fileStreamTraverse.Field("buf_recycle");
                var buf_recycle = buf_recycleTraverse.GetValue<byte[]>();
                if (buf.Length == 4096 && buf_recycle == null)
                {
                    var obj = fileStreamTraverse.Field("buf_recycle_lock").GetValue();
                    lock (obj)
                    {
                        _ = buf_recycleTraverse.SetValue(buf);
                    }
                }

                _ = bufTraverse.SetValue(null);
                GC.SuppressFinalize(__instance);
            }

            if (ex != null)
            {
                throw ex;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.Position), MethodType.Setter)]
    private class set_Position
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref FileStreamOrig __instance, long value)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Non-negative number required.");
            }

            _ = __instance.Seek(value, SeekOrigin.Begin);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), "WriteInternal")]
    private class WriteInternal
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref FileStreamOrig __instance, byte[] src, ref int offset, ref int count)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            var instanceTraverse = Traverse.Create(__instance);
            var flushBufferTraverse = instanceTraverse.Method("FlushBuffer");
            var buf_startTraverse = instanceTraverse.Field("buf_start");
            var name = instanceTraverse.Field("name").GetValue<string>();

            if (count > instanceTraverse.Field("buf_size").GetValue<int>())
            {
                _ = flushBufferTraverse.GetValue();
                if (__instance.CanSeek && !instanceTraverse.Field("isExposed").GetValue<bool>())
                {
                    _ = FileSystem.OsHelpers.Seek(name, buf_startTraverse.GetValue<long>(), SeekOrigin.Begin);
                }

                var i = count;
                while (i > 0)
                {
                    var num = FileSystem.OsHelpers.Write(name, src, offset, i);
                    i -= num;
                    offset += num;
                }

                _ = buf_startTraverse.SetValue(buf_startTraverse.GetValue<long>() + count);
                return false;
            }

            var num2 = 0;
            while (count > 0)
            {
                var num3 = instanceTraverse.Method("WriteSegment", new[] { typeof(byte[]), typeof(int), typeof(int) })
                    .GetValue<int>(src, offset + num2, count);
                num2 += num3;
                count -= num3;
                if (count == 0)
                {
                    break;
                }

                _ = flushBufferTraverse.GetValue();
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.WriteByte))]
    private class WriteByte
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref FileStreamOrig __instance, byte value)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            if (!__instance.CanWrite)
            {
                throw new NotSupportedException("Stream does not support writing");
            }

            var instanceTraverse = Traverse.Create(__instance);
            var buf_offsetTraverse = instanceTraverse.Field("buf_offset");
            var bufTraverse = instanceTraverse.Field("buf");
            var buf_sizeTraverse = instanceTraverse.Field("buf_size");
            var buf_lengthTraverse = instanceTraverse.Field("buf_length");
            var buf_dirtyTraverse = instanceTraverse.Field("buf_dirty");
            var flushBufferTraverse = instanceTraverse.Method("FlushBuffer");
            if (buf_offsetTraverse.GetValue<int>() == buf_sizeTraverse.GetValue<int>())
            {
                _ = flushBufferTraverse.GetValue();
            }

            if (buf_sizeTraverse.GetValue<int>() == 0)
            {
                bufTraverse.GetValue<byte[]>()[0] = value;
                _ = buf_dirtyTraverse.SetValue(true);
                _ = buf_lengthTraverse.SetValue(1);
                _ = flushBufferTraverse.GetValue();
                return false;
            }

            var array = bufTraverse.GetValue<byte[]>();
            var num = buf_offsetTraverse.GetValue<int>();
            _ = buf_offsetTraverse.SetValue(num + 1);
            array[num] = value;
            if (buf_offsetTraverse.GetValue<int>() > buf_lengthTraverse.GetValue<int>())
            {
                _ = buf_lengthTraverse.SetValue(buf_offsetTraverse.GetValue<int>());
            }

            _ = buf_dirtyTraverse.SetValue(true);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.Unlock))]
    private class Unlock
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix()
        {
            return Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.Lock))]
    private class Lock
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix()
        {
            return Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.SetLength))]
    private class SetLength
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref FileStreamOrig __instance, long value)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            if (!__instance.CanSeek)
            {
                throw new NotSupportedException("The stream does not support seeking");
            }

            if (!__instance.CanWrite)
            {
                throw new NotSupportedException("The stream does not support writing");
            }

            if (value < 0L)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "value is less than 0");
            }

            var instanceTraverse = Traverse.Create(__instance);
            _ = instanceTraverse.Method("FlushBuffer").GetValue();
            FileSystem.OsHelpers.SetLength(instanceTraverse.Field("name").GetValue<string>(), value);
            if (__instance.Position > value)
            {
                __instance.Position = value;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.SetAccessControl))]
    private class SetAccessControl
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix()
        {
            return Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), "ReadData")]
    private class ReadData
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref FileStreamOrig __instance, ref int __result, byte[] buf, int offset, int count)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            var num = FileSystem.OsHelpers.Read(Traverse.Create(__instance).Field("name").GetValue<string>(), buf,
                offset, count);
            __result = num;
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.ReadByte))]
    private class ReadByte
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref int __result, ref FileStreamOrig __instance)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            if (!__instance.CanRead)
            {
                throw new NotSupportedException("Stream does not support reading");
            }

            var instanceTraverse = Traverse.Create(__instance);
            var buf_lengthTraverse = instanceTraverse.Field("buf_length");
            var buf_offsetTraverse = instanceTraverse.Field("buf_offset");
            var bufTraverse = instanceTraverse.Field("buf");
            if (instanceTraverse.Field("buf_size").GetValue<int>() != 0)
            {
                if (buf_offsetTraverse.GetValue<int>() >= buf_lengthTraverse.GetValue<int>())
                {
                    _ = instanceTraverse.Method("RefillBuffer").GetValue();
                    if (buf_lengthTraverse.GetValue<int>() == 0)
                    {
                        __result = -1;
                        return false;
                    }
                }

                var array = bufTraverse.GetValue<byte[]>();
                var num = buf_offsetTraverse.GetValue<int>();
                _ = buf_offsetTraverse.SetValue(num + 1);
                __result = array[num];
                return false;
            }

            if (instanceTraverse.Method("ReadData").GetValue<int>(null, bufTraverse.GetValue<byte[]>(), 0, 1) == 0)
            {
                __result = -1;
                return false;
            }

            __result = bufTraverse.GetValue<byte[]>()[0];
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.Read))]
    private class Read
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref int __result, ref FileStreamOrig __instance, [In] [Out] byte[] array, int offset,
            int count)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (!__instance.CanRead)
            {
                throw new NotSupportedException("Stream does not support reading");
            }

            var num = array.Length;
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "< 0");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "< 0");
            }

            if (offset > num)
            {
                throw new ArgumentException("destination offset is beyond array size");
            }

            if (offset > num - count)
            {
                throw new ArgumentException("Reading would overrun buffer");
            }

            var instanceTraverse = Traverse.Create(__instance);
            if (instanceTraverse.Field("async").GetValue<bool>())
            {
                var asyncResult = __instance.BeginRead(array, offset, count, null, null);
                __result = __instance.EndRead(asyncResult);
                return false;
            }

            __result = instanceTraverse.Method("ReadInternal", new[] { typeof(byte[]), typeof(int), typeof(int) })
                .GetValue<int>(array, offset, count);
            return false;
        }
    }

#pragma warning disable CS0618 // Type or member is obsolete
    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.Handle), MethodType.Getter)]
#pragma warning restore CS0618 // Type or member is obsolete
    private class get_Handle
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref IntPtr __result, ref FileStreamOrig __instance)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            var instanceTraverse = Traverse.Create(__instance);
            if (!instanceTraverse.Field("isExposed").GetValue<bool>())
            {
                _ = instanceTraverse.Method("ExposeHandle").GetValue();
            }

            __result = IntPtr.Zero;
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.GetAccessControl))]
    private class GetAccessControl
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref object __result)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            __result = null;
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), "FlushBuffer")]
    private class FlushBuffer
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref FileStreamOrig __instance)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            var instanceTraverse = Traverse.Create(__instance);
            var buf_dirtyTraverse = instanceTraverse.Field("buf_dirty");
            var buf_startTraverse = instanceTraverse.Field("buf_start");
            var buf_offsetTraverse = instanceTraverse.Field("buf_offset");
            var buf_lengthTraverse = instanceTraverse.Field("buf_length");
            if (buf_dirtyTraverse.GetValue<bool>())
            {
                var name = instanceTraverse.Field("name").GetValue<string>();
                if (__instance.CanSeek && !instanceTraverse.Field("isExposed").GetValue<bool>())
                {
                    _ = FileSystem.OsHelpers.Seek(name, buf_startTraverse.GetValue<long>(), SeekOrigin.Begin);
                }

                var i = buf_lengthTraverse.GetValue<int>();
                var num = 0;
                while (i > 0)
                {
                    var num2 = FileSystem.OsHelpers.Write(name, instanceTraverse.Field("buf").GetValue<byte[]>(), num,
                        buf_lengthTraverse.GetValue<int>());
                    i -= num2;
                    num += num2;
                }
            }

            _ = buf_startTraverse.SetValue(buf_startTraverse.GetValue<long>() + buf_offsetTraverse.GetValue<int>());
            _ = buf_lengthTraverse.SetValue(0);
            _ = buf_offsetTraverse.SetValue(0);
            _ = buf_dirtyTraverse.SetValue(false);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), "FlushAsync")]
    private class FlushAsync
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref FileStreamOrig __instance, ref object __result, object cancellationToken)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            var flushMethod = AccessTools.Method(typeof(FileStreamOrig), "FlushAsync");
            __result = flushMethod.GetBaseDefinition().Invoke(__instance, new[] { cancellationToken });
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.Flush), typeof(bool))]
    private class Flush__bool
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref FileStreamOrig __instance)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            _ = Traverse.Create(__instance).Method("FlushBuffer").GetValue();
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.Flush), new Type[0])]
    private class Flush
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref FileStreamOrig __instance)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            _ = Traverse.Create(__instance).Method("FlushBuffer").GetValue();
            return false;
        }
    }

    private delegate void WriteDelegate(object instance, byte[] buffer, int offset, int count);

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.BeginWrite))]
    private class BeginWrite
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static void writeInternal(object instance, byte[] src, int offset, int count)
        {
            _ = Traverse.Create(instance).Method("WriteInternal", new[] { typeof(byte[]), typeof(int), typeof(int) })
                .GetValue(src, offset, count);
        }

        private static bool Prefix(ref IAsyncResult __result, ref FileStreamOrig __instance, byte[] array, int offset,
            int numBytes, AsyncCallback userCallback, object stateObject)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            if (!__instance.CanWrite)
            {
                throw new NotSupportedException("This stream does not support writing");
            }

            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (numBytes < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numBytes), "Must be >= 0");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Must be >= 0");
            }

            if (numBytes > array.Length - offset)
            {
                throw new ArgumentException("array too small. numBytes/offset wrong.");
            }

            var instanceTraverse = Traverse.Create(__instance);
            if (!instanceTraverse.Field("async").GetValue<bool>())
            {
                var beginWrite = AccessTools.Method(typeof(FileStreamOrig), "BeginWrite");
                __result = (IAsyncResult)beginWrite.GetBaseDefinition().Invoke(__instance,
                    new[] { array, offset, numBytes, userCallback, stateObject });
                return false;
            }

            var fileStreamAsyncResultType = AccessTools.TypeByName("System.IO.FileStreamAsyncResult");
            var fileStreamAsyncResultCtor = AccessTools.Constructor(fileStreamAsyncResultType);
            var fileStreamAsyncResult = fileStreamAsyncResultCtor.Invoke(new[] { userCallback, stateObject });
            var fileStreamAsyncResultTraverse = Traverse.Create(fileStreamAsyncResult);
            _ = fileStreamAsyncResultTraverse.Field("BytesRead").SetValue(-1);
            _ = fileStreamAsyncResultTraverse.Field("Count").SetValue(numBytes);
            _ = fileStreamAsyncResultTraverse.Field("OriginalCount").SetValue(numBytes);
            __result = new WriteDelegate(writeInternal).BeginInvoke(__instance, array, offset, numBytes, userCallback,
                stateObject);
            return false;
        }
    }

    private delegate int ReadDelegate(object instance, byte[] buffer, int offset, int count);

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.BeginRead))]
    private class BeginRead
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static int readInternal(object instance, byte[] dest, int offset, int count)
        {
            return Traverse.Create(instance).Method("ReadInternal", new[] { typeof(byte[]), typeof(int), typeof(int) })
                .GetValue<int>(dest, offset, count);
        }

        private static bool Prefix(ref IAsyncResult __result, ref FileStreamOrig __instance, byte[] array, int offset,
            int numBytes, AsyncCallback userCallback, object stateObject)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            if (!__instance.CanRead)
            {
                throw new NotSupportedException("This stream does not support reading");
            }

            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (numBytes < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numBytes), "Must be >= 0");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Must be >= 0");
            }

            if (numBytes > array.Length - offset)
            {
                throw new ArgumentException("Buffer too small. numBytes/offset wrong.");
            }

            var instanceTraverse = Traverse.Create(__instance);
            if (!instanceTraverse.Field("async").GetValue<bool>())
            {
                var beginRead = AccessTools.Method(typeof(FileStreamOrig), "BeginRead");
                __result = (IAsyncResult)beginRead.GetBaseDefinition().Invoke(__instance,
                    new[] { array, offset, numBytes, userCallback, stateObject });
                return false;
            }

            __result = new ReadDelegate(readInternal).BeginInvoke(__instance, array, offset, numBytes, userCallback,
                stateObject);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), "Init")]
    private class Init
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref FileStreamOrig __instance, FileAccess access, bool ownsHandle, int bufferSize,
            bool isAsync, bool isConsoleWrapper)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            if (access is < FileAccess.Read or > FileAccess.ReadWrite)
            {
                throw new ArgumentOutOfRangeException(nameof(access));
            }

            if (!isConsoleWrapper && bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "Positive number required.");
            }

            var instanceTraverse = Traverse.Create(__instance);
            _ = instanceTraverse.Field("canseek").SetValue(true);
            _ = instanceTraverse.Method("ExposeHandle").GetValue();
            _ = instanceTraverse.Field("access").SetValue(access);
            _ = instanceTraverse.Field("owner").SetValue(ownsHandle);
            _ = instanceTraverse.Field("async").SetValue(isAsync);
            _ = instanceTraverse.Field("anonymous").SetValue(false);
            _ = instanceTraverse.Field("buf_start")
                .SetValue(FileSystem.OsHelpers.Seek(instanceTraverse.Field("name").GetValue<string>(), 0,
                    SeekOrigin.Current));
            _ = instanceTraverse.Field("append_startpos").SetValue(0);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), MethodType.Constructor, typeof(IntPtr), typeof(FileAccess), typeof(bool),
        typeof(int), typeof(bool), typeof(bool))]
    private class ctor__IntPtr__FileAccess__bool__int__bool__bool
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix()
        {
            return Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking
                ? true
                : throw new InvalidOperationException(
                    "This constructor is not supported by the virtual file system, if this happens then patch more methods to prevent this.");
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), MethodType.Constructor, typeof(SafeFileHandle), typeof(FileAccess),
        typeof(int), typeof(bool))]
    private class ctor__SafeFileHandle__FileAccess__int_bool
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix()
        {
            return Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking
                ? true
                : throw new InvalidOperationException(
                    "This constructor is not supported by the virtual file system, if this happens then patch more methods to prevent this.");
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.EndRead))]
    private class EndRead
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref int __result, ref FileStreamOrig __instance, IAsyncResult asyncResult)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            if (asyncResult == null)
            {
                throw new ArgumentNullException(nameof(asyncResult));
            }

            var instanceTraverse = Traverse.Create(__instance);
            if (!instanceTraverse.Field("async").GetValue<bool>())
            {
                var endRead = AccessTools.Method(typeof(FileStreamOrig), "EndRead", new[] { typeof(IAsyncResult) });
                return (bool)endRead.GetBaseDefinition().Invoke(__instance, new object[] { asyncResult });
            }

            if (asyncResult is not AsyncResult asyncResult2)
            {
                throw new ArgumentException("Invalid IAsyncResult", nameof(asyncResult));
            }

            if (asyncResult2.AsyncDelegate is not ReadDelegate readDelegate)
            {
                throw new ArgumentException("Invalid IAsyncResult", nameof(asyncResult));
            }

            __result = readDelegate.EndInvoke(asyncResult);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.EndWrite))]
    private class EndWrite
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref FileStreamOrig __instance, IAsyncResult asyncResult)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            if (asyncResult == null)
            {
                throw new ArgumentNullException(nameof(asyncResult));
            }

            var instanceTraverse = Traverse.Create(__instance);
            if (!instanceTraverse.Field("async").GetValue<bool>())
            {
                var endRead = AccessTools.Method(typeof(FileStreamOrig), "EndWrite", new[] { typeof(IAsyncResult) });
                _ = endRead.Invoke(__instance, new object[] { asyncResult });
                return false;
            }

            if (asyncResult is not AsyncResult asyncResult2)
            {
                throw new ArgumentException("Invalid IAsyncResult", nameof(asyncResult));
            }

            if (asyncResult2.AsyncDelegate is not WriteDelegate writeDelegate)
            {
                throw new ArgumentException("Invalid IAsyncResult", nameof(asyncResult));
            }

            writeDelegate.EndInvoke(asyncResult);
            return false;
        }
    }
}