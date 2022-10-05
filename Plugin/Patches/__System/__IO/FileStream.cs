using HarmonyLib;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using UniTASPlugin.FakeGameState.GameFileSystem;
using FileStreamOrig = System.IO.FileStream;

namespace UniTASPlugin.Patches.__System.__IO;

[HarmonyPatch]
public static class FileStream
{
    static class Helper
    {
        public static bool CallOriginal()
        {
            var trace = new System.Diagnostics.StackTrace();
            var traceFrames = trace.GetFrames();
            foreach (var frame in traceFrames)
            {
                var typeName = frame.GetMethod().DeclaringType.FullName;
                if (typeName.StartsWith("BepInEx.Logging") || typeName.StartsWith("UniTASPlugin.ReversePatches"))
                {
                    return true;
                }
            }
            return false;
        }

        public static Traverse WriteInternalTraverse(FileStreamOrig instance)
        {
            return Traverse.Create(instance).Method("WriteInternal", new Type[] { typeof(byte[]), typeof(int), typeof(int) });
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), MethodType.Constructor, new Type[] { typeof(string), typeof(FileMode), typeof(FileAccess), typeof(FileShare), typeof(int), typeof(bool), typeof(FileOptions) })]
    class Ctor__string__FileMode__FileAccess__FileShare__int__bool__FileOptions
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref FileStreamOrig __instance, ref string path, FileMode mode, FileAccess access, ref FileShare share, ref int bufferSize, bool anonymous, FileOptions options)
        {
            if (Helper.CallOriginal())
                return true;
            var instanceTraverse = Traverse.Create(__instance);
            instanceTraverse.Field("name").SetValue("[Unknown]");
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException("Path is empty");
            }
            instanceTraverse.Field("anonymous").SetValue(anonymous);
            share &= ~FileShare.Inheritable;
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize", "Positive number required.");
            }
            if (mode < FileMode.CreateNew || mode > FileMode.Append)
            {
                if (anonymous)
                {
                    throw new ArgumentException("mode", "Enum value was out of legal range.");
                }
                throw new ArgumentOutOfRangeException("mode", "Enum value was out of legal range.");
            }
            else
            {
                if (access < FileAccess.Read || access > FileAccess.ReadWrite)
                {
                    throw new ArgumentOutOfRangeException("access", "Enum value was out of legal range.");
                }
                if (share < FileShare.None || share > (FileShare.Read | FileShare.Write | FileShare.Delete))
                {
                    throw new ArgumentOutOfRangeException("share", "Enum value was out of legal range.");
                }
#pragma warning disable CS0618 // Type or member is obsolete
                if (path.IndexOfAny(Path.InvalidPathChars) != -1)
                {
                    throw new ArgumentException("Name has invalid chars");
                }
#pragma warning restore CS0618 // Type or member is obsolete
                var pathTraverse = Traverse.Create(typeof(Path));
                path = pathTraverse.Method("InsecureGetFullPath", new Type[] { typeof(string) }).GetValue<string>(path);
                if (Directory.Exists(path))
                {
                    var getSecureFileName = Traverse.Create(typeof(FileStreamOrig)).Method("GetSecureFileName", new Type[] { typeof(string) });
                    throw new UnauthorizedAccessException(string.Format("Access to the path '{0}' is denied.", getSecureFileName.GetValue(new object[] { path, false })));
                }
                if (mode == FileMode.Append && (access & FileAccess.Read) == FileAccess.Read)
                {
                    throw new ArgumentException("Append access can be requested only in write-only mode.");
                }
                if ((access & FileAccess.Write) == 0 && mode != FileMode.Open && mode != FileMode.OpenOrCreate)
                {
                    throw new ArgumentException(string.Format("Combining FileMode: {0} with FileAccess: {1} is invalid.", access, mode));
                }
                string directoryName = Path.GetDirectoryName(path);
                if (directoryName.Length > 0 && !Directory.Exists(Path.GetFullPath(directoryName)))
                {
                    string arg = anonymous ? directoryName : Path.GetFullPath(path);
                    throw new DirectoryNotFoundException(string.Format("Could not find a part of the path \"{0}\".", arg));
                }
                //if (!anonymous)
                //{
                instanceTraverse.Field("name").SetValue(path);
                //}
                FileSystem.OsHelpers.OpenFile(path, mode, access, share, options);
                instanceTraverse.Field("access").SetValue(access);
                instanceTraverse.Field("owner").SetValue(true);
                instanceTraverse.Field("canseek").SetValue(true);
                instanceTraverse.Field("async").SetValue((options & FileOptions.Asynchronous) > FileOptions.None);
                if (access == FileAccess.Read && bufferSize == 4096)
                {
                    long length = __instance.Length;
                    if (bufferSize > length)
                    {
                        bufferSize = (int)((length < 1000L) ? 1000L : length);
                    }
                }
                instanceTraverse.Method("InitBuffer", new Type[] { typeof(int), typeof(bool) }).GetValue(bufferSize, false);
                if (mode == FileMode.Append)
                {
                    __instance.Seek(0L, SeekOrigin.End);
                    instanceTraverse.Field("append_startpos").SetValue(__instance.Position);
                    return false;
                }
                instanceTraverse.Field("append_startpos").SetValue(0L);
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.Seek))]
    class Seek
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref long __result, ref FileStreamOrig __instance, long offset, SeekOrigin origin)
        {
            if (Helper.CallOriginal())
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
                _ => throw new ArgumentException("origin", "Invalid SeekOrigin"),
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
            instanceTraverse.Method("FlushBuffer").GetValue();
            var seekResult = FileSystem.OsHelpers.Seek(instanceTraverse.Field("name").GetValue<string>(), num, SeekOrigin.Begin);
            instanceTraverse.Field("buf_start").SetValue(seekResult);
            __result = seekResult;
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.Length), MethodType.Getter)]
    class get_Length
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref long __result, ref FileStreamOrig __instance)
        {
            if (Helper.CallOriginal())
                return true;
            if (!__instance.CanSeek)
            {
                throw new NotSupportedException("The stream does not support seeking");
            }
            var instanceTraverse = Traverse.Create(__instance);
            instanceTraverse.Method("FlushBufferIfDirty").GetValue();
            __result = FileSystem.OsHelpers.Length(instanceTraverse.Field("name").GetValue<string>());
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.Position), MethodType.Getter)]
    class get_Position
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref long __result, ref FileStreamOrig __instance)
        {
            if (Helper.CallOriginal())
                return true;
            if (!__instance.CanSeek)
            {
                throw new NotSupportedException("The stream does not support seeking");
            }
            var instanceTraverse = Traverse.Create(__instance);
            if (!instanceTraverse.Field("isExposed").GetValue<bool>())
            {
                __result = instanceTraverse.Field("buf_start").GetValue<long>() + instanceTraverse.Field("buf_offset").GetValue<int>();
                return false;
            }
            __result = FileSystem.OsHelpers.Seek(instanceTraverse.Field("name").GetValue<string>(), 0, SeekOrigin.Current);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.Write))]
    class Write
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref FileStreamOrig __instance, byte[] array, int offset, int count)
        {
            if (Helper.CallOriginal())
                return true;
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "< 0");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "< 0");
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
                IAsyncResult asyncResult = __instance.BeginWrite(array, offset, count, null, null);
                __instance.EndWrite(asyncResult);
                return false;
            }
            Helper.WriteInternalTraverse(__instance).GetValue(array, offset, count);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.Dispose))]
    class Dispose
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref FileStreamOrig __instance, bool disposing)
        {
            if (Helper.CallOriginal())
                return true;
            Exception ex = null;
            var instanceTraverse = Traverse.Create(__instance);
            var name = instanceTraverse.Field("name").GetValue<string>();
            FileSystem.OsHelpers.Close(name);
            instanceTraverse.Field("canseek").SetValue(false);
            instanceTraverse.Field("access").SetValue((FileAccess)0);
            var bufTraverse = instanceTraverse.Field("buf");
            var buf = bufTraverse.GetValue<byte[]>();
            if (disposing && buf != null)
            {
                var fileStreamTraverse = Traverse.Create(typeof(FileStreamOrig));
                var buf_recycleTraverse = fileStreamTraverse.Field("buf_recycle");
                var buf_recycle = buf_recycleTraverse.GetValue<byte[]>();
                if (buf.Length == 4096 && buf_recycle == null)
                {
                    object obj = fileStreamTraverse.Field("buf_recycle_lock").GetValue();
                    lock (obj)
                    {
                        if (buf_recycle == null)
                        {
                            buf_recycleTraverse.SetValue(buf);
                        }
                    }
                }
                bufTraverse.SetValue(null);
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
    class set_Position
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref FileStreamOrig __instance, long value)
        {
            if (Helper.CallOriginal())
                return true;
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException("value", "Non-negative number required.");
            }
            __instance.Seek(value, SeekOrigin.Begin);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), "WriteInternal")]
    class WriteInternal
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref FileStreamOrig __instance, byte[] src, ref int offset, ref int count)
        {
            if (Helper.CallOriginal())
                return true;
            var instanceTraverse = Traverse.Create(__instance);
            var flushBufferTraverse = instanceTraverse.Method("FlushBuffer");
            var buf_startTraverse = instanceTraverse.Field("buf_start");
            var name = instanceTraverse.Field("name").GetValue<string>();

            if (count > instanceTraverse.Field("buf_size").GetValue<int>())
            {
                flushBufferTraverse.GetValue();
                if (__instance.CanSeek && !instanceTraverse.Field("isExposed").GetValue<bool>())
                {
                    FileSystem.OsHelpers.Seek(name, buf_startTraverse.GetValue<long>(), SeekOrigin.Begin);
                }
                int i = count;
                while (i > 0)
                {
                    var num = FileSystem.OsHelpers.Write(name, src, offset, i);
                    i -= num;
                    offset += num;
                }
                buf_startTraverse.SetValue(buf_startTraverse.GetValue<long>() + count);
                return false;
            }
            int num2 = 0;
            while (count > 0)
            {
                var num3 = instanceTraverse.Method("WriteSegment", new Type[] { typeof(byte[]), typeof(int), typeof(int) }).GetValue<int>(src, offset + num2, count);
                num2 += num3;
                count -= num3;
                if (count == 0)
                {
                    break;
                }
                flushBufferTraverse.GetValue();
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.WriteByte))]
    class WriteByte
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref FileStreamOrig __instance, byte value)
        {
            if (Helper.CallOriginal())
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
                flushBufferTraverse.GetValue();
            }
            if (buf_sizeTraverse.GetValue<int>() == 0)
            {
                bufTraverse.GetValue<byte[]>()[0] = value;
                buf_dirtyTraverse.SetValue(true);
                buf_lengthTraverse.SetValue(1);
                flushBufferTraverse.GetValue();
                return false;
            }
            byte[] array = bufTraverse.GetValue<byte[]>();
            int num = buf_offsetTraverse.GetValue<int>();
            buf_offsetTraverse.SetValue(num + 1);
            array[num] = value;
            if (buf_offsetTraverse.GetValue<int>() > buf_lengthTraverse.GetValue<int>())
            {
                buf_lengthTraverse.SetValue(buf_offsetTraverse.GetValue<int>());
            }
            buf_dirtyTraverse.SetValue(true);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.Unlock))]
    class Unlock
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix()
        {
            if (Helper.CallOriginal())
                return true;
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.Lock))]
    class Lock
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix()
        {
            if (Helper.CallOriginal())
                return true;
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.SetLength))]
    class SetLength
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref FileStreamOrig __instance, long value)
        {
            if (Helper.CallOriginal())
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
                throw new ArgumentOutOfRangeException("value is less than 0");
            }
            var instanceTraverse = Traverse.Create(__instance);
            instanceTraverse.Method("FlushBuffer").GetValue();
            FileSystem.OsHelpers.SetLength(instanceTraverse.Field("name").GetValue<string>(), value);
            if (__instance.Position > value)
            {
                __instance.Position = value;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.SetAccessControl))]
    class SetAccessControl
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix()
        {
            if (Helper.CallOriginal())
                return true;
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), "ReadData")]
    class ReadData
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref FileStreamOrig __instance, ref int __result, byte[] buf, int offset, int count)
        {
            if (Helper.CallOriginal())
                return true;
            int num = FileSystem.OsHelpers.Read(Traverse.Create(__instance).Field("name").GetValue<string>(), buf, offset, count);
            __result = num;
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.ReadByte))]
    class ReadByte
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref int __result, ref FileStreamOrig __instance)
        {
            if (Helper.CallOriginal())
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
                    instanceTraverse.Method("RefillBuffer").GetValue();
                    if (buf_lengthTraverse.GetValue<int>() == 0)
                    {
                        __result = -1;
                        return false;
                    }
                }
                byte[] array = bufTraverse.GetValue<byte[]>();
                var num = buf_offsetTraverse.GetValue<int>();
                buf_offsetTraverse.SetValue(num + 1);
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
    class Read
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref int __result, ref FileStreamOrig __instance, [In][Out] byte[] array, int offset, int count)
        {
            if (Helper.CallOriginal())
                return true;
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (!__instance.CanRead)
            {
                throw new NotSupportedException("Stream does not support reading");
            }
            int num = array.Length;
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "< 0");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "< 0");
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
                IAsyncResult asyncResult = __instance.BeginRead(array, offset, count, null, null);
                __result = __instance.EndRead(asyncResult);
                return false;
            }
            __result = instanceTraverse.Method("ReadInternal", new Type[] { typeof(byte[]), typeof(int), typeof(int) }).GetValue<int>(array, offset, count);
            return false;
        }
    }

#pragma warning disable CS0618 // Type or member is obsolete
    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.Handle), MethodType.Getter)]
#pragma warning restore CS0618 // Type or member is obsolete
    class get_Handle
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref IntPtr __result, ref FileStreamOrig __instance)
        {
            if (Helper.CallOriginal())
                return true;
            var instanceTraverse = Traverse.Create(__instance);
            if (!instanceTraverse.Field("isExposed").GetValue<bool>())
            {
                instanceTraverse.Method("ExposeHandle").GetValue();
            }
            __result = IntPtr.Zero;
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.GetAccessControl))]
    class GetAccessControl
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref object __result)
        {
            if (Helper.CallOriginal())
                return true;
            __result = null;
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), "FlushBuffer")]
    class FlushBuffer
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref FileStreamOrig __instance)
        {
            if (Helper.CallOriginal())
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
                    FileSystem.OsHelpers.Seek(name, buf_startTraverse.GetValue<long>(), SeekOrigin.Begin);
                }
                int i = buf_lengthTraverse.GetValue<int>();
                int num = 0;
                while (i > 0)
                {
                    var num2 = FileSystem.OsHelpers.Write(name, instanceTraverse.Field("buf").GetValue<byte[]>(), num, buf_lengthTraverse.GetValue<int>());
                    i -= num2;
                    num += num2;
                }
            }
            buf_startTraverse.SetValue(buf_startTraverse.GetValue<long>() + buf_offsetTraverse.GetValue<int>());
            buf_lengthTraverse.SetValue(0);
            buf_offsetTraverse.SetValue(0);
            buf_dirtyTraverse.SetValue(false);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), "FlushAsync")]
    class FlushAsync
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref FileStreamOrig __instance, ref object __result, object cancellationToken)
        {
            if (Helper.CallOriginal())
                return true;
            var flushMethod = AccessTools.Method(typeof(FileStreamOrig), "FlushAsync");
            __result = flushMethod.GetBaseDefinition().Invoke(__instance, new object[] { cancellationToken });
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.Flush), new Type[] { typeof(bool) })]
    class Flush__bool
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref FileStreamOrig __instance)
        {
            if (Helper.CallOriginal())
                return true;
            Traverse.Create(__instance).Method("FlushBuffer").GetValue();
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.Flush), new Type[0])]
    class Flush
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref FileStreamOrig __instance)
        {
            if (Helper.CallOriginal())
                return true;
            Traverse.Create(__instance).Method("FlushBuffer").GetValue();
            return false;
        }
    }

    delegate void WriteDelegate(object instance, byte[] buffer, int offset, int count);

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.BeginWrite))]
    class BeginWrite
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static void writeInternal(object instance, byte[] src, int offset, int count)
        {
            Traverse.Create(instance).Method("WriteInternal", new Type[] { typeof(byte[]), typeof(int), typeof(int) }).GetValue(src, offset, count);
        }

        static bool Prefix(ref IAsyncResult __result, ref FileStreamOrig __instance, byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
        {
            if (Helper.CallOriginal())
                return true;
            if (!__instance.CanWrite)
            {
                throw new NotSupportedException("This stream does not support writing");
            }
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (numBytes < 0)
            {
                throw new ArgumentOutOfRangeException("numBytes", "Must be >= 0");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "Must be >= 0");
            }
            if (numBytes > array.Length - offset)
            {
                throw new ArgumentException("array too small. numBytes/offset wrong.");
            }
            var instanceTraverse = Traverse.Create(__instance);
            if (!instanceTraverse.Field("async").GetValue<bool>())
            {
                var beginWrite = AccessTools.Method(typeof(FileStreamOrig), "BeginWrite");
                __result = (IAsyncResult)beginWrite.GetBaseDefinition().Invoke(__instance, new object[] { array, offset, numBytes, userCallback, stateObject });
                return false;
            }
            var fileStreamAsyncResultType = AccessTools.TypeByName("System.IO.FileStreamAsyncResult");
            var fileStreamAsyncResultCtor = AccessTools.Constructor(fileStreamAsyncResultType);
            var fileStreamAsyncResult = fileStreamAsyncResultCtor.Invoke(new object[] { userCallback, stateObject });
            var fileStreamAsyncResultTraverse = Traverse.Create(fileStreamAsyncResult);
            fileStreamAsyncResultTraverse.Field("BytesRead").SetValue(-1);
            fileStreamAsyncResultTraverse.Field("Count").SetValue(numBytes);
            fileStreamAsyncResultTraverse.Field("OriginalCount").SetValue(numBytes);
            __result = new WriteDelegate(writeInternal).BeginInvoke(__instance, array, offset, numBytes, userCallback, stateObject);
            return false;
        }
    }

    delegate int ReadDelegate(object instance, byte[] buffer, int offset, int count);

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.BeginRead))]
    class BeginRead
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static int readInternal(object instance, byte[] dest, int offset, int count)
        {
            return Traverse.Create(instance).Method("ReadInternal", new Type[] { typeof(byte[]), typeof(int), typeof(int) }).GetValue<int>(dest, offset, count);
        }

        static bool Prefix(ref IAsyncResult __result, ref FileStreamOrig __instance, byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
        {
            if (Helper.CallOriginal())
                return true;
            if (!__instance.CanRead)
            {
                throw new NotSupportedException("This stream does not support reading");
            }
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (numBytes < 0)
            {
                throw new ArgumentOutOfRangeException("numBytes", "Must be >= 0");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "Must be >= 0");
            }
            if (numBytes > array.Length - offset)
            {
                throw new ArgumentException("Buffer too small. numBytes/offset wrong.");
            }
            var instanceTraverse = Traverse.Create(__instance);
            if (!instanceTraverse.Field("async").GetValue<bool>())
            {
                var beginRead = AccessTools.Method(typeof(FileStreamOrig), "BeginRead");
                __result = (IAsyncResult)beginRead.GetBaseDefinition().Invoke(__instance, new object[] { array, offset, numBytes, userCallback, stateObject });
                return false;
            }
            __result = new ReadDelegate(readInternal).BeginInvoke(__instance, array, offset, numBytes, userCallback, stateObject);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), "Init")]
    class Init
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref FileStreamOrig __instance, FileAccess access, bool ownsHandle, int bufferSize, bool isAsync, bool isConsoleWrapper)
        {
            if (Helper.CallOriginal())
                return true;
            if (access < FileAccess.Read || access > FileAccess.ReadWrite)
            {
                throw new ArgumentOutOfRangeException("access");
            }
            if (!isConsoleWrapper && bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize", "Positive number required.");
            }
            var instanceTraverse = Traverse.Create(__instance);
            instanceTraverse.Field("canseek").SetValue(true);
            instanceTraverse.Method("ExposeHandle").GetValue();
            instanceTraverse.Field("access").SetValue(access);
            instanceTraverse.Field("owner").SetValue(ownsHandle);
            instanceTraverse.Field("async").SetValue(isAsync);
            instanceTraverse.Field("anonymous").SetValue(false);
            instanceTraverse.Field("buf_start").SetValue(FileSystem.OsHelpers.Seek(instanceTraverse.Field("name").GetValue<string>(), 0, SeekOrigin.Current));
            instanceTraverse.Field("append_startpos").SetValue(0);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), MethodType.Constructor, new Type[] { typeof(IntPtr), typeof(FileAccess), typeof(bool), typeof(int), typeof(bool), typeof(bool) })]
    class ctor__IntPtr__FileAccess__bool__int__bool__boool
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix()
        {
            if (Helper.CallOriginal())
                return true;
            throw new InvalidOperationException("This constructor is not supported by the virtual file system, if this happens then patch more methods to prevent this.");
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), MethodType.Constructor, new Type[] { typeof(SafeFileHandle), typeof(FileAccess), typeof(int), typeof(bool) })]
    class ctor__SafeFileHandle__FileAccess__int_bool
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix()
        {
            if (Helper.CallOriginal())
                return true;
            throw new InvalidOperationException("This constructor is not supported by the virtual file system, if this happens then patch more methods to prevent this.");
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.EndRead))]
    class EndRead
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref int __result, ref FileStreamOrig __instance, IAsyncResult asyncResult)
        {
            if (Helper.CallOriginal())
                return true;
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            var instanceTraverse = Traverse.Create(__instance);
            if (!instanceTraverse.Field("async").GetValue<bool>())
            {
                var endRead = AccessTools.Method(typeof(FileStreamOrig), "EndRead", new Type[] { typeof(IAsyncResult) });
                return (bool)endRead.GetBaseDefinition().Invoke(__instance, new object[] { asyncResult });
            }
            if (asyncResult is not AsyncResult asyncResult2)
            {
                throw new ArgumentException("Invalid IAsyncResult", "asyncResult");
            }
            if (asyncResult2.AsyncDelegate is not ReadDelegate readDelegate)
            {
                throw new ArgumentException("Invalid IAsyncResult", "asyncResult");
            }
            __result = readDelegate.EndInvoke(asyncResult);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileStreamOrig), nameof(FileStreamOrig.EndWrite))]
    class EndWrite
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref FileStreamOrig __instance, IAsyncResult asyncResult)
        {
            if (Helper.CallOriginal())
                return true;
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            var instanceTraverse = Traverse.Create(__instance);
            if (!instanceTraverse.Field("async").GetValue<bool>())
            {
                var endRead = AccessTools.Method(typeof(FileStreamOrig), "EndWrite", new Type[] { typeof(IAsyncResult) });
                endRead.Invoke(__instance, new object[] { asyncResult });
                return false;
            }
            if (asyncResult is not AsyncResult asyncResult2)
            {
                throw new ArgumentException("Invalid IAsyncResult", "asyncResult");
            }
            if (asyncResult2.AsyncDelegate is not WriteDelegate writeDelegate)
            {
                throw new ArgumentException("Invalid IAsyncResult", "asyncResult");
            }
            writeDelegate.EndInvoke(asyncResult);
            return false;
        }
    }
}