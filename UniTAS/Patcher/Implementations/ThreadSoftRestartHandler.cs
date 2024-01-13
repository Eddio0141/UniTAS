using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BepInEx.Logging;
using UniTAS.Patcher.Implementations.TASRenderer;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations;

[Singleton]
public class ThreadSoftRestartHandler : IThreadTracker, IOnPreGameRestart
{
    private readonly List<Thread> _threads = [];
    private readonly ILogger _logger;

    private readonly Type[] _excludeTypes =
    [
        typeof(DiskLogListener),
        typeof(GameVideoRenderer),
        typeof(NativeAudioRenderer),
        typeof(LoggingUtils.DiskLogger)
    ];

    public ThreadSoftRestartHandler(ILogger logger)
    {
        _logger = logger;
    }

    public void ThreadStart(Thread thread)
    {
        if (_threads.Contains(thread)) return;

        var trace = new StackTrace();
        var allFrames = trace.GetFrames();

        // skip first 3 frames, this method, prefix, and harmony wrapper
        // var frame = trace.FrameCount > 2 ? trace.GetFrame(3) : null;
        if (allFrames == null) return;
        var frames = allFrames.Skip(3).ToArray();

        if (frames.Any(x => _excludeTypes.Contains(x.GetMethod()?.DeclaringType)))
        {
            _logger.LogDebug(
                $"Ignoring thread start, name: {thread.Name}, ID: {thread.ManagedThreadId}, stack trace: {trace}");
            return;
        }

        _logger.LogDebug(
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
                _logger.LogDebug($"Exception thrown while aborting thread: {e}");
            }
        }

        _threads.Clear();

        foreach (var thread in pendingJoin)
        {
            thread.Join();

            _logger.LogDebug($"Interrupted game thread, name: {thread.Name}, ID: {thread.ManagedThreadId}");
        }

        _logger.LogDebug("All game threads interrupted");
    }
}