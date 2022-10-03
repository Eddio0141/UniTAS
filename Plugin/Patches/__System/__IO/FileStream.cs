using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UniTASPlugin.FakeGameState.GameFileSystem;
using FileStreamOrig = System.IO.FileStream;

namespace UniTASPlugin.Patches.__System.__IO;

[HarmonyPatch(typeof(FileStreamOrig), MethodType.Constructor, new Type[] { typeof(string), typeof(FileMode), typeof(FileAccess), typeof(FileShare), typeof(int), typeof(bool), typeof(FileOptions) })]
class Ctor__string__FileMode__FileAccess__FileShare__int__bool__FileOptions
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref FileStreamOrig __instance, ref string path, FileMode mode, FileAccess access, ref FileShare share, ref int bufferSize, bool anonymous, FileOptions options)
    {
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
            if ((access & FileAccess.Write) == (FileAccess)0 && mode != FileMode.Open && mode != FileMode.OpenOrCreate)
            {
                throw new ArgumentException(string.Format("Combining FileMode: {0} with FileAccess: {1} is invalid.", access, mode));
            }
            string directoryName = Path.GetDirectoryName(path);
            if (directoryName.Length > 0 && !Directory.Exists(Path.GetFullPath(directoryName)))
            {
                string arg = anonymous ? directoryName : Path.GetFullPath(path);
                throw new DirectoryNotFoundException(string.Format("Could not find a part of the path \"{0}\".", arg));
            }
            if (!anonymous)
            {
                instanceTraverse.Field("name").SetValue(path);
            }
            FileSystem.OsHelpers.OpenFile(path, mode, access, share, options);
            // no safe handle field
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
        if (!__instance.CanSeek)
        {
            throw new NotSupportedException("The stream does not support seeking");
        }
        long num;
        switch (origin)
        {
            case SeekOrigin.Begin:
                num = offset;
                break;
            case SeekOrigin.Current:
                num = __instance.Position + offset;
                break;
            case SeekOrigin.End:
                num = __instance.Length + offset;
                break;
            default:
                throw new ArgumentException("origin", "Invalid SeekOrigin");
        }
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
        MonoIOError monoIOError;
        __instance.buf_start = MonoIO.Seek(__instance.safeHandle, num, SeekOrigin.Begin, out monoIOError);
        __result = instanceTraverse.Field("buf_start").GetValue<long>();
        return false;
    }
}

class Dummy4
{
    /*
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    internal FileStream(IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize, bool isAsync, bool isConsoleWrapper)
    {
        this.name = "[Unknown]";
        base..ctor();
        if (handle == MonoIO.InvalidHandle)
        {
            throw new ArgumentException("handle", Locale.GetText("Invalid."));
        }
        this.Init(new SafeFileHandle(handle, false), access, ownsHandle, bufferSize, isAsync, isConsoleWrapper);
    }

    /// <summary>Initializes a new instance of the <see cref="T:System.IO.FileStream" /> class for the specified file handle, with the specified read/write permission, buffer size, and synchronous or asynchronous state.</summary>
    /// <param name="handle">A file handle for the file that this <see langword="FileStream" /> object will encapsulate.</param>
    /// <param name="access">A constant that sets the <see cref="P:System.IO.FileStream.CanRead" /> and <see cref="P:System.IO.FileStream.CanWrite" /> properties of the <see langword="FileStream" /> object.</param>
    /// <param name="bufferSize">A positive <see cref="T:System.Int32" /> value greater than 0 indicating the buffer size. The default buffer size is 4096.</param>
    /// <param name="isAsync">
    ///   <see langword="true" /> if the handle was opened asynchronously (that is, in overlapped I/O mode); otherwise, <see langword="false" />.</param>
    /// <exception cref="T:System.ArgumentException">The <paramref name="handle" /> parameter is an invalid handle.  
    ///  -or-  
    ///  The <paramref name="handle" /> parameter is a synchronous handle and it was used asynchronously.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="bufferSize" /> parameter is negative.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error, such as a disk error, occurred.  
    ///  -or-  
    ///  The stream has been closed.</exception>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="access" /> requested is not permitted by the operating system for the specified file handle, such as when <paramref name="access" /> is <see langword="Write" /> or <see langword="ReadWrite" /> and the file handle is set for read-only access.</exception>
    public FileStream(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync)
    {
        this.name = "[Unknown]";
        base..ctor();
        this.Init(handle, access, false, bufferSize, isAsync, false);
    }

    private void Init(SafeFileHandle safeHandle, FileAccess access, bool ownsHandle, int bufferSize, bool isAsync, bool isConsoleWrapper)
    {
        if (!isConsoleWrapper && safeHandle.IsInvalid)
        {
            throw new ArgumentException(Environment.GetResourceString("Invalid handle."), "handle");
        }
        if (access < FileAccess.Read || access > FileAccess.ReadWrite)
        {
            throw new ArgumentOutOfRangeException("access");
        }
        if (!isConsoleWrapper && bufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("Positive number required."));
        }
        MonoIOError monoIOError;
        MonoFileType fileType = MonoIO.GetFileType(safeHandle, out monoIOError);
        if (monoIOError != MonoIOError.ERROR_SUCCESS)
        {
            throw MonoIO.GetException(this.name, monoIOError);
        }
        if (fileType == MonoFileType.Unknown)
        {
            throw new IOException("Invalid handle.");
        }
        if (fileType == MonoFileType.Disk)
        {
            this.canseek = true;
        }
        else
        {
            this.canseek = false;
        }
        this.safeHandle = safeHandle;
        this.ExposeHandle();
        this.access = access;
        this.owner = ownsHandle;
        this.async = isAsync;
        this.anonymous = false;
        if (this.canseek)
        {
            this.buf_start = MonoIO.Seek(safeHandle, 0L, SeekOrigin.Current, out monoIOError);
            if (monoIOError != MonoIOError.ERROR_SUCCESS)
            {
                throw MonoIO.GetException(this.name, monoIOError);
            }
        }
        this.append_startpos = 0L;
    }
    
    /// <summary>Gets the length in bytes of the stream.</summary>
    /// <returns>A long value representing the length of the stream in bytes.</returns>
    /// <exception cref="T:System.NotSupportedException">
    ///   <see cref="P:System.IO.FileStream.CanSeek" /> for this stream is <see langword="false" />.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error, such as the file being closed, occurred.</exception>
    // (get) Token: 0x06002ABD RID: 10941 RVA: 0x00095EB8 File Offset: 0x000940B8
    public override long Length
    {
        get
        {
            if (this.safeHandle.IsClosed)
            {
                throw new ObjectDisposedException("Stream has been closed");
            }
            if (!this.CanSeek)
            {
                throw new NotSupportedException("The stream does not support seeking");
            }
            this.FlushBufferIfDirty();
            MonoIOError monoIOError;
            long length = MonoIO.GetLength(this.safeHandle, out monoIOError);
            if (monoIOError != MonoIOError.ERROR_SUCCESS)
            {
                throw MonoIO.GetException(this.GetSecureFileName(this.name), monoIOError);
            }
            return length;
        }
    }

    /// <summary>Gets or sets the current position of this stream.</summary>
    /// <returns>The current position of this stream.</returns>
    /// <exception cref="T:System.NotSupportedException">The stream does not support seeking.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurred.  
    /// -or-
    ///  The position was set to a very large value beyond the end of the stream in Windows 98 or earlier.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Attempted to set the position to a negative value.</exception>
    /// <exception cref="T:System.IO.EndOfStreamException">Attempted seeking past the end of a stream that does not support this.</exception>
    // (get) Token: 0x06002ABE RID: 10942 RVA: 0x00095F1C File Offset: 0x0009411C
    // (set) Token: 0x06002ABF RID: 10943 RVA: 0x00095F91 File Offset: 0x00094191
    public override long Position
    {
        get
        {
            if (this.safeHandle.IsClosed)
            {
                throw new ObjectDisposedException("Stream has been closed");
            }
            if (!this.CanSeek)
            {
                throw new NotSupportedException("The stream does not support seeking");
            }
            if (!this.isExposed)
            {
                return this.buf_start + (long)this.buf_offset;
            }
            MonoIOError monoIOError;
            long result = MonoIO.Seek(this.safeHandle, 0L, SeekOrigin.Current, out monoIOError);
            if (monoIOError != MonoIOError.ERROR_SUCCESS)
            {
                throw MonoIO.GetException(this.GetSecureFileName(this.name), monoIOError);
            }
            return result;
        }
        set
        {
            if (value < 0L)
            {
                throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("Non-negative number required."));
            }
            this.Seek(value, SeekOrigin.Begin);
        }
    }

    /// <summary>Gets the operating system file handle for the file that the current <see langword="FileStream" /> object encapsulates.</summary>
    /// <returns>The operating system file handle for the file encapsulated by this <see langword="FileStream" /> object, or -1 if the <see langword="FileStream" /> has been closed.</returns>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
    // (get) Token: 0x06002AC0 RID: 10944 RVA: 0x00095FB6 File Offset: 0x000941B6
    [Obsolete("Use SafeFileHandle instead")]
    public virtual IntPtr Handle
    {
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
        get
        {
            IntPtr result = this.safeHandle.DangerousGetHandle();
            if (!this.isExposed)
            {
                this.ExposeHandle();
            }
            return result;
        }
    }

    /// <summary>Gets a <see cref="T:Microsoft.Win32.SafeHandles.SafeFileHandle" /> object that represents the operating system file handle for the file that the current <see cref="T:System.IO.FileStream" /> object encapsulates.</summary>
    /// <returns>An object that represents the operating system file handle for the file that the current <see cref="T:System.IO.FileStream" /> object encapsulates.</returns>
    // (get) Token: 0x06002AC1 RID: 10945 RVA: 0x00095FD1 File Offset: 0x000941D1
    public virtual SafeFileHandle SafeFileHandle
    {
        [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        get
        {
            if (!this.isExposed)
            {
                this.ExposeHandle();
            }
            return this.safeHandle;
        }
    }

    private void ExposeHandle()
    {
        this.isExposed = true;
        this.FlushBuffer();
        this.InitBuffer(0, true);
    }

    /// <summary>Reads a byte from the file and advances the read position one byte.</summary>
    /// <returns>The byte, cast to an <see cref="T:System.Int32" />, or -1 if the end of the stream has been reached.</returns>
    /// <exception cref="T:System.NotSupportedException">The current stream does not support reading.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The current stream is closed.</exception>
    public override int ReadByte()
    {
        if (this.safeHandle.IsClosed)
        {
            throw new ObjectDisposedException("Stream has been closed");
        }
        if (!this.CanRead)
        {
            throw new NotSupportedException("Stream does not support reading");
        }
        if (this.buf_size != 0)
        {
            if (this.buf_offset >= this.buf_length)
            {
                this.RefillBuffer();
                if (this.buf_length == 0)
                {
                    return -1;
                }
            }
            byte[] array = this.buf;
            int num = this.buf_offset;
            this.buf_offset = num + 1;
            return array[num];
        }
        if (this.ReadData(this.safeHandle, this.buf, 0, 1) == 0)
        {
            return -1;
        }
        return (int)this.buf[0];
    }

    /// <summary>Writes a byte to the current position in the file stream.</summary>
    /// <param name="value">A byte to write to the stream.</param>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.NotSupportedException">The stream does not support writing.</exception>
    public override void WriteByte(byte value)
    {
        if (this.safeHandle.IsClosed)
        {
            throw new ObjectDisposedException("Stream has been closed");
        }
        if (!this.CanWrite)
        {
            throw new NotSupportedException("Stream does not support writing");
        }
        if (this.buf_offset == this.buf_size)
        {
            this.FlushBuffer();
        }
        if (this.buf_size == 0)
        {
            this.buf[0] = value;
            this.buf_dirty = true;
            this.buf_length = 1;
            this.FlushBuffer();
            return;
        }
        byte[] array = this.buf;
        int num = this.buf_offset;
        this.buf_offset = num + 1;
        array[num] = value;
        if (this.buf_offset > this.buf_length)
        {
            this.buf_length = this.buf_offset;
        }
        this.buf_dirty = true;
    }

    /// <summary>Reads a block of bytes from the stream and writes the data in a given buffer.</summary>
    /// <param name="array">When this method returns, contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
    /// <param name="offset">The byte offset in <paramref name="array" /> at which the read bytes will be placed.</param>
    /// <param name="count">The maximum number of bytes to read.</param>
    /// <returns>The total number of bytes read into the buffer. This might be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="array" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
    /// <exception cref="T:System.NotSupportedException">The stream does not support reading.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="offset" /> and <paramref name="count" /> describe an invalid range in <paramref name="array" />.</exception>
    /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
    public override int Read([In][Out] byte[] array, int offset, int count)
    {
        if (this.safeHandle.IsClosed)
        {
            throw new ObjectDisposedException("Stream has been closed");
        }
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }
        if (!this.CanRead)
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
        if (this.async)
        {
            IAsyncResult asyncResult = this.BeginRead(array, offset, count, null, null);
            return this.EndRead(asyncResult);
        }
        return this.ReadInternal(array, offset, count);
    }

    private int ReadInternal(byte[] dest, int offset, int count)
    {
        int num = this.ReadSegment(dest, offset, count);
        if (num == count)
        {
            return count;
        }
        int num2 = num;
        count -= num;
        if (count > this.buf_size)
        {
            this.FlushBuffer();
            num = this.ReadData(this.safeHandle, dest, offset + num, count);
            this.buf_start += (long)num;
        }
        else
        {
            this.RefillBuffer();
            num = this.ReadSegment(dest, offset + num2, count);
        }
        return num2 + num;
    }

    /// <summary>Begins an asynchronous read operation. Consider using <see cref="M:System.IO.FileStream.ReadAsync(System.Byte[],System.Int32,System.Int32,System.Threading.CancellationToken)" /> instead.</summary>
    /// <param name="array">The buffer to read data into.</param>
    /// <param name="offset">The byte offset in <paramref name="array" /> at which to begin reading.</param>
    /// <param name="numBytes">The maximum number of bytes to read.</param>
    /// <param name="userCallback">The method to be called when the asynchronous read operation is completed.</param>
    /// <param name="stateObject">A user-provided object that distinguishes this particular asynchronous read request from other requests.</param>
    /// <returns>An object that references the asynchronous read.</returns>
    /// <exception cref="T:System.ArgumentException">The array length minus <paramref name="offset" /> is less than <paramref name="numBytes" />.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="array" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="offset" /> or <paramref name="numBytes" /> is negative.</exception>
    /// <exception cref="T:System.IO.IOException">An asynchronous read was attempted past the end of the file.</exception>
    public override IAsyncResult BeginRead(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
    {
        if (this.safeHandle.IsClosed)
        {
            throw new ObjectDisposedException("Stream has been closed");
        }
        if (!this.CanRead)
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
        if (!this.async)
        {
            return base.BeginRead(array, offset, numBytes, userCallback, stateObject);
        }
        return new FileStream.ReadDelegate(this.ReadInternal).BeginInvoke(array, offset, numBytes, userCallback, stateObject);
    }

    /// <summary>Waits for the pending asynchronous read operation to complete. (Consider using <see cref="M:System.IO.FileStream.ReadAsync(System.Byte[],System.Int32,System.Int32,System.Threading.CancellationToken)" /> instead.)</summary>
    /// <param name="asyncResult">The reference to the pending asynchronous request to wait for.</param>
    /// <returns>The number of bytes read from the stream, between 0 and the number of bytes you requested. Streams only return 0 at the end of the stream, otherwise, they should block until at least 1 byte is available.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="asyncResult" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">This <see cref="T:System.IAsyncResult" /> object was not created by calling <see cref="M:System.IO.FileStream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" /> on this class.</exception>
    /// <exception cref="T:System.InvalidOperationException">
    ///   <see cref="M:System.IO.FileStream.EndRead(System.IAsyncResult)" /> is called multiple times.</exception>
    /// <exception cref="T:System.IO.IOException">The stream is closed or an internal error has occurred.</exception>
    public override int EndRead(IAsyncResult asyncResult)
    {
        if (asyncResult == null)
        {
            throw new ArgumentNullException("asyncResult");
        }
        if (!this.async)
        {
            return base.EndRead(asyncResult);
        }
        AsyncResult asyncResult2 = asyncResult as AsyncResult;
        if (asyncResult2 == null)
        {
            throw new ArgumentException("Invalid IAsyncResult", "asyncResult");
        }
        FileStream.ReadDelegate readDelegate = asyncResult2.AsyncDelegate as FileStream.ReadDelegate;
        if (readDelegate == null)
        {
            throw new ArgumentException("Invalid IAsyncResult", "asyncResult");
        }
        return readDelegate.EndInvoke(asyncResult);
    }

    /// <summary>Writes a block of bytes to the file stream.</summary>
    /// <param name="array">The buffer containing data to write to the stream.</param>
    /// <param name="offset">The zero-based byte offset in <paramref name="array" /> from which to begin copying bytes to the stream.</param>
    /// <param name="count">The maximum number of bytes to write.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="array" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="offset" /> and <paramref name="count" /> describe an invalid range in <paramref name="array" />.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurred.  
    /// -or-
    ///  Another thread may have caused an unexpected change in the position of the operating system's file handle.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.NotSupportedException">The current stream instance does not support writing.</exception>
    public override void Write(byte[] array, int offset, int count)
    {
        if (this.safeHandle.IsClosed)
        {
            throw new ObjectDisposedException("Stream has been closed");
        }
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
        if (!this.CanWrite)
        {
            throw new NotSupportedException("Stream does not support writing");
        }
        if (this.async)
        {
            IAsyncResult asyncResult = this.BeginWrite(array, offset, count, null, null);
            this.EndWrite(asyncResult);
            return;
        }
        this.WriteInternal(array, offset, count);
    }

    private void WriteInternal(byte[] src, int offset, int count)
    {
        if (count > this.buf_size)
        {
            this.FlushBuffer();
            if (this.CanSeek && !this.isExposed)
            {
                MonoIOError monoIOError;
                MonoIO.Seek(this.safeHandle, this.buf_start, SeekOrigin.Begin, out monoIOError);
                if (monoIOError != MonoIOError.ERROR_SUCCESS)
                {
                    throw MonoIO.GetException(this.GetSecureFileName(this.name), monoIOError);
                }
            }
            int i = count;
            while (i > 0)
            {
                MonoIOError monoIOError;
                int num = MonoIO.Write(this.safeHandle, src, offset, i, out monoIOError);
                if (monoIOError != MonoIOError.ERROR_SUCCESS)
                {
                    throw MonoIO.GetException(this.GetSecureFileName(this.name), monoIOError);
                }
                i -= num;
                offset += num;
            }
            this.buf_start += (long)count;
            return;
        }
        int num2 = 0;
        while (count > 0)
        {
            int num3 = this.WriteSegment(src, offset + num2, count);
            num2 += num3;
            count -= num3;
            if (count == 0)
            {
                break;
            }
            this.FlushBuffer();
        }
    }

    /// <summary>Begins an asynchronous write operation. Consider using <see cref="M:System.IO.FileStream.WriteAsync(System.Byte[],System.Int32,System.Int32,System.Threading.CancellationToken)" /> instead.</summary>
    /// <param name="array">The buffer containing data to write to the current stream.</param>
    /// <param name="offset">The zero-based byte offset in <paramref name="array" /> at which to begin copying bytes to the current stream.</param>
    /// <param name="numBytes">The maximum number of bytes to write.</param>
    /// <param name="userCallback">The method to be called when the asynchronous write operation is completed.</param>
    /// <param name="stateObject">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
    /// <returns>An object that references the asynchronous write.</returns>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="array" /> length minus <paramref name="offset" /> is less than <paramref name="numBytes" />.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="array" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="offset" /> or <paramref name="numBytes" /> is negative.</exception>
    /// <exception cref="T:System.NotSupportedException">The stream does not support writing.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
    public override IAsyncResult BeginWrite(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
    {
        if (this.safeHandle.IsClosed)
        {
            throw new ObjectDisposedException("Stream has been closed");
        }
        if (!this.CanWrite)
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
        if (!this.async)
        {
            return base.BeginWrite(array, offset, numBytes, userCallback, stateObject);
        }
        FileStreamAsyncResult fileStreamAsyncResult = new FileStreamAsyncResult(userCallback, stateObject);
        fileStreamAsyncResult.BytesRead = -1;
        fileStreamAsyncResult.Count = numBytes;
        fileStreamAsyncResult.OriginalCount = numBytes;
        return new FileStream.WriteDelegate(this.WriteInternal).BeginInvoke(array, offset, numBytes, userCallback, stateObject);
    }

    /// <summary>Ends an asynchronous write operation and blocks until the I/O operation is complete. (Consider using <see cref="M:System.IO.FileStream.WriteAsync(System.Byte[],System.Int32,System.Int32,System.Threading.CancellationToken)" /> instead.)</summary>
    /// <param name="asyncResult">The pending asynchronous I/O request.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="asyncResult" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">This <see cref="T:System.IAsyncResult" /> object was not created by calling <see cref="M:System.IO.Stream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)" /> on this class.</exception>
    /// <exception cref="T:System.InvalidOperationException">
    ///   <see cref="M:System.IO.FileStream.EndWrite(System.IAsyncResult)" /> is called multiple times.</exception>
    /// <exception cref="T:System.IO.IOException">The stream is closed or an internal error has occurred.</exception>
    public override void EndWrite(IAsyncResult asyncResult)
    {
        if (asyncResult == null)
        {
            throw new ArgumentNullException("asyncResult");
        }
        if (!this.async)
        {
            base.EndWrite(asyncResult);
            return;
        }
        AsyncResult asyncResult2 = asyncResult as AsyncResult;
        if (asyncResult2 == null)
        {
            throw new ArgumentException("Invalid IAsyncResult", "asyncResult");
        }
        FileStream.WriteDelegate writeDelegate = asyncResult2.AsyncDelegate as FileStream.WriteDelegate;
        if (writeDelegate == null)
        {
            throw new ArgumentException("Invalid IAsyncResult", "asyncResult");
        }
        writeDelegate.EndInvoke(asyncResult);
    }

    /// <summary>Sets the length of this stream to the given value.</summary>
    /// <param name="value">The new length of the stream.</param>
    /// <exception cref="T:System.IO.IOException">An I/O error has occurred.</exception>
    /// <exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Attempted to set the <paramref name="value" /> parameter to less than 0.</exception>
    public override void SetLength(long value)
    {
        if (this.safeHandle.IsClosed)
        {
            throw new ObjectDisposedException("Stream has been closed");
        }
        if (!this.CanSeek)
        {
            throw new NotSupportedException("The stream does not support seeking");
        }
        if (!this.CanWrite)
        {
            throw new NotSupportedException("The stream does not support writing");
        }
        if (value < 0L)
        {
            throw new ArgumentOutOfRangeException("value is less than 0");
        }
        this.FlushBuffer();
        MonoIOError monoIOError;
        MonoIO.SetLength(this.safeHandle, value, out monoIOError);
        if (monoIOError != MonoIOError.ERROR_SUCCESS)
        {
            throw MonoIO.GetException(this.GetSecureFileName(this.name), monoIOError);
        }
        if (this.Position > value)
        {
            this.Position = value;
        }
    }

    /// <summary>Clears buffers for this stream and causes any buffered data to be written to the file.</summary>
    /// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    public override void Flush()
    {
        if (this.safeHandle.IsClosed)
        {
            throw new ObjectDisposedException("Stream has been closed");
        }
        this.FlushBuffer();
    }

    /// <summary>Clears buffers for this stream and causes any buffered data to be written to the file, and also clears all intermediate file buffers.</summary>
    /// <param name="flushToDisk">
    ///   <see langword="true" /> to flush all intermediate file buffers; otherwise, <see langword="false" />.</param>
    public virtual void Flush(bool flushToDisk)
    {
        if (this.safeHandle.IsClosed)
        {
            throw new ObjectDisposedException("Stream has been closed");
        }
        this.FlushBuffer();
        if (flushToDisk)
        {
            MonoIOError monoIOError;
            MonoIO.Flush(this.safeHandle, out monoIOError);
        }
    }

    /// <summary>Prevents other processes from reading from or writing to the <see cref="T:System.IO.FileStream" />.</summary>
    /// <param name="position">The beginning of the range to lock. The value of this parameter must be equal to or greater than zero (0).</param>
    /// <param name="length">The range to be locked.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="position" /> or <paramref name="length" /> is negative.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The file is closed.</exception>
    /// <exception cref="T:System.IO.IOException">The process cannot access the file because another process has locked a portion of the file.</exception>
    public virtual void Lock(long position, long length)
    {
        if (this.safeHandle.IsClosed)
        {
            throw new ObjectDisposedException("Stream has been closed");
        }
        if (position < 0L)
        {
            throw new ArgumentOutOfRangeException("position must not be negative");
        }
        if (length < 0L)
        {
            throw new ArgumentOutOfRangeException("length must not be negative");
        }
        MonoIOError monoIOError;
        MonoIO.Lock(this.safeHandle, position, length, out monoIOError);
        if (monoIOError != MonoIOError.ERROR_SUCCESS)
        {
            throw MonoIO.GetException(this.GetSecureFileName(this.name), monoIOError);
        }
    }

    /// <summary>Allows access by other processes to all or part of a file that was previously locked.</summary>
    /// <param name="position">The beginning of the range to unlock.</param>
    /// <param name="length">The range to be unlocked.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="position" /> or <paramref name="length" /> is negative.</exception>
    public virtual void Unlock(long position, long length)
    {
        if (this.safeHandle.IsClosed)
        {
            throw new ObjectDisposedException("Stream has been closed");
        }
        if (position < 0L)
        {
            throw new ArgumentOutOfRangeException("position must not be negative");
        }
        if (length < 0L)
        {
            throw new ArgumentOutOfRangeException("length must not be negative");
        }
        MonoIOError monoIOError;
        MonoIO.Unlock(this.safeHandle, position, length, out monoIOError);
        if (monoIOError != MonoIOError.ERROR_SUCCESS)
        {
            throw MonoIO.GetException(this.GetSecureFileName(this.name), monoIOError);
        }
    }

    /// <summary>Ensures that resources are freed and other cleanup operations are performed when the garbage collector reclaims the <see langword="FileStream" />.</summary>
    ~FileStream()
    {
        this.Dispose(false);
    }

    /// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.FileStream" /> and optionally releases the managed resources.</summary>
    /// <param name="disposing">
    ///   <see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        Exception ex = null;
        if (this.safeHandle != null && !this.safeHandle.IsClosed)
        {
            try
            {
                this.FlushBuffer();
            }
            catch (Exception ex)
            {
            }
            if (this.owner)
            {
                MonoIOError monoIOError;
                MonoIO.Close(this.safeHandle.DangerousGetHandle(), out monoIOError);
                if (monoIOError != MonoIOError.ERROR_SUCCESS)
                {
                    throw MonoIO.GetException(this.GetSecureFileName(this.name), monoIOError);
                }
                this.safeHandle.DangerousRelease();
            }
        }
        this.canseek = false;
        this.access = (FileAccess)0;
        if (disposing && this.buf != null)
        {
            if (this.buf.Length == 4096 && FileStream.buf_recycle == null)
            {
                object obj = FileStream.buf_recycle_lock;
                lock (obj)
                {
                    if (FileStream.buf_recycle == null)
                    {
                        FileStream.buf_recycle = this.buf;
                    }
                }
            }
            this.buf = null;
            GC.SuppressFinalize(this);
        }
        if (ex != null)
        {
            throw ex;
        }
    }

    /// <summary>Gets a <see cref="T:System.Security.AccessControl.FileSecurity" /> object that encapsulates the access control list (ACL) entries for the file described by the current <see cref="T:System.IO.FileStream" /> object.</summary>
    /// <returns>An object that encapsulates the access control settings for the file described by the current <see cref="T:System.IO.FileStream" /> object.</returns>
    /// <exception cref="T:System.ObjectDisposedException">The file is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
    /// <exception cref="T:System.SystemException">The file could not be found.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">This operation is not supported on the current platform.  
    ///  -or-  
    ///  The caller does not have the required permission.</exception>
    public FileSecurity GetAccessControl()
    {
        if (this.safeHandle.IsClosed)
        {
            throw new ObjectDisposedException("Stream has been closed");
        }
        return new FileSecurity(this.SafeFileHandle, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
    }

    /// <summary>Applies access control list (ACL) entries described by a <see cref="T:System.Security.AccessControl.FileSecurity" /> object to the file described by the current <see cref="T:System.IO.FileStream" /> object.</summary>
    /// <param name="fileSecurity">An object that describes an ACL entry to apply to the current file.</param>
    /// <exception cref="T:System.ObjectDisposedException">The file is closed.</exception>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="fileSecurity" /> parameter is <see langword="null" />.</exception>
    /// <exception cref="T:System.SystemException">The file could not be found or modified.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The current process does not have access to open the file.</exception>
    public void SetAccessControl(FileSecurity fileSecurity)
    {
        if (this.safeHandle.IsClosed)
        {
            throw new ObjectDisposedException("Stream has been closed");
        }
        if (fileSecurity == null)
        {
            throw new ArgumentNullException("fileSecurity");
        }
        fileSecurity.PersistModifications(this.SafeFileHandle);
    }

    /// <summary>Asynchronously clears all buffers for this stream, causes any buffered data to be written to the underlying device, and monitors cancellation requests.</summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous flush operation.</returns>
    /// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        if (this.safeHandle.IsClosed)
        {
            throw new ObjectDisposedException("Stream has been closed");
        }
        return base.FlushAsync(cancellationToken);
    }

    /// <summary>Asynchronously reads a sequence of bytes from the current stream, advances the position within the stream by the number of bytes read, and monitors cancellation requests.</summary>
    /// <param name="buffer">The buffer to write the data into.</param>
    /// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin writing data from the stream.</param>
    /// <param name="count">The maximum number of bytes to read.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="buffer" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
    /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length.</exception>
    /// <exception cref="T:System.NotSupportedException">The stream does not support reading.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
    /// <exception cref="T:System.InvalidOperationException">The stream is currently in use by a previous read operation.</exception>
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return base.ReadAsync(buffer, offset, count, cancellationToken);
    }

    /// <summary>Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream by the number of bytes written, and monitors cancellation requests.</summary>
    /// <param name="buffer">The buffer to write data from.</param>
    /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> from which to begin copying bytes to the stream.</param>
    /// <param name="count">The maximum number of bytes to write.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="buffer" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
    /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length.</exception>
    /// <exception cref="T:System.NotSupportedException">The stream does not support writing.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
    /// <exception cref="T:System.InvalidOperationException">The stream is currently in use by a previous write operation.</exception>
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return base.WriteAsync(buffer, offset, count, cancellationToken);
    }

    private int ReadSegment(byte[] dest, int dest_offset, int count)
    {
        count = Math.Min(count, this.buf_length - this.buf_offset);
        if (count > 0)
        {
            Buffer.InternalBlockCopy(this.buf, this.buf_offset, dest, dest_offset, count);
            this.buf_offset += count;
        }
        return count;
    }

    private int WriteSegment(byte[] src, int src_offset, int count)
    {
        if (count > this.buf_size - this.buf_offset)
        {
            count = this.buf_size - this.buf_offset;
        }
        if (count > 0)
        {
            Buffer.BlockCopy(src, src_offset, this.buf, this.buf_offset, count);
            this.buf_offset += count;
            if (this.buf_offset > this.buf_length)
            {
                this.buf_length = this.buf_offset;
            }
            this.buf_dirty = true;
        }
        return count;
    }

    private void FlushBuffer()
    {
        if (this.buf_dirty)
        {
            if (this.CanSeek && !this.isExposed)
            {
                MonoIOError monoIOError;
                MonoIO.Seek(this.safeHandle, this.buf_start, SeekOrigin.Begin, out monoIOError);
                if (monoIOError != MonoIOError.ERROR_SUCCESS)
                {
                    throw MonoIO.GetException(this.GetSecureFileName(this.name), monoIOError);
                }
            }
            int i = this.buf_length;
            int num = 0;
            while (i > 0)
            {
                MonoIOError monoIOError;
                int num2 = MonoIO.Write(this.safeHandle, this.buf, num, this.buf_length, out monoIOError);
                if (monoIOError != MonoIOError.ERROR_SUCCESS)
                {
                    throw MonoIO.GetException(this.GetSecureFileName(this.name), monoIOError);
                }
                i -= num2;
                num += num2;
            }
        }
        this.buf_start += (long)this.buf_offset;
        this.buf_offset = (this.buf_length = 0);
        this.buf_dirty = false;
    }

    private void FlushBufferIfDirty()
    {
        if (this.buf_dirty)
        {
            this.FlushBuffer();
        }
    }

    private void RefillBuffer()
    {
        this.FlushBuffer();
        this.buf_length = this.ReadData(this.safeHandle, this.buf, 0, this.buf_size);
    }

    private int ReadData(SafeHandle safeHandle, byte[] buf, int offset, int count)
    {
        MonoIOError monoIOError;
        int num = MonoIO.Read(safeHandle, buf, offset, count, out monoIOError);
        if (monoIOError == MonoIOError.ERROR_BROKEN_PIPE)
        {
            num = 0;
        }
        else if (monoIOError != MonoIOError.ERROR_SUCCESS)
        {
            throw MonoIO.GetException(this.GetSecureFileName(this.name), monoIOError);
        }
        if (num == -1)
        {
            throw new IOException();
        }
        return num;
    }

    internal const int DefaultBufferSize = 4096;

    private static byte[] buf_recycle;

    private static readonly object buf_recycle_lock = new object();

    private byte[] buf;

    private string name;

    private SafeFileHandle safeHandle;

    private bool isExposed;

    private long append_startpos;

    private FileAccess access;

    private bool owner;

    private bool async;

    private bool canseek;

    private bool anonymous;

    private bool buf_dirty;

    private int buf_size;

    private int buf_length;

    private int buf_offset;

    private long buf_start;
    */
}