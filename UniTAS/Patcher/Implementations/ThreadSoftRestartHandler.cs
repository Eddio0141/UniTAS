using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BepInEx.Logging;
using UniTAS.Patcher.Implementations.Customization;
using UniTAS.Patcher.Implementations.TASRenderer;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Interfaces.Movie;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations;

[Singleton]
public class ThreadSoftRestartHandler(ILogger logger) : IThreadTracker, IOnPreGameRestart
{
    private readonly List<Thread> _threads = [];

    private readonly Type[] _excludeTypes =
    [
        typeof(DiskLogListener),
        typeof(GameVideoRenderer),
        typeof(NativeAudioRenderer),
        typeof(LoggingUtils.DiskLogger),
        typeof(RemoteControl),
        typeof(ReadOnlyFieldDescriptor),
        typeof(Config)
    ];

    public void ThreadStart(Thread thread)
    {
        if (_threads.Contains(thread)) return;

        var trace = new StackTrace();
        var allFrames = trace.GetFrames();

        // skip first 3 frames, this method, prefix, and harmony wrapper
        // var frame = trace.FrameCount > 2 ? trace.GetFrame(3) : null;
        if (allFrames == null) return;
        var frames = allFrames.Skip(3).ToArray();

        // intentionally don't check if UniTAS namespace is included, any exclusions for UniTAS is to be included in _excludeTypes manually
        if (frames.Any(x => _excludeTypes.Contains(x.GetMethod()?.DeclaringType)))
        {
            logger.LogDebug(
                $"Ignoring thread start, name: {thread.Name}, ID: {thread.ManagedThreadId}, stack trace: {trace}");
            return;
        }

        logger.LogDebug(
            $"Tracking thread start, name: {thread.Name}, ID: {thread.ManagedThreadId}, stack trace: {trace}");
        _threads.Add(thread);
    }

    public void OnPreGameRestart()
    {
        var pendingJoin = new List<Thread>();

        foreach (var thread in _threads)
        {
            if (!thread.IsAlive) continue;

            try
            {
                thread.Interrupt();
                pendingJoin.Add(thread);
            }
            catch (Exception e)
            {
                logger.LogDebug($"Exception thrown while aborting thread: {e}");
            }
        }

        _threads.Clear();

        foreach (var thread in pendingJoin)
        {
            thread.Join();

            logger.LogDebug($"Interrupted game thread, name: {thread.Name}, ID: {thread.ManagedThreadId}");
        }

        logger.LogDebug("All game threads interrupted");
    }
}