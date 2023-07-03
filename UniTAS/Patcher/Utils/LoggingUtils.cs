using System.IO;
using System.Threading;
using BepInEx;
using BepInEx.Logging;

namespace UniTAS.Patcher.Utils;

public static class LoggingUtils
{
    public static void InitDiskLogger()
    {
        var diskLogger = new DiskLogger();
        if (!diskLogger.Enabled) return;

        Logger.Listeners.Add(diskLogger);
    }

    // separate disk logging
    public class DiskLogger : ILogListener
    {
        private readonly TextWriter _logWriter;
        private readonly Timer _flushTimer;

        public bool Enabled { get; } = true;

        public DiskLogger()
        {
            if (!Utility.TryOpenFileStream(Path.Combine(Paths.BepInExRootPath, "UniTAS.log"), FileMode.Create,
                    out var fileStream, FileAccess.Write))
            {
                StaticLogger.Log.LogError("Couldn't open a log file for writing. Skipping UniTAS log file creation");
                Enabled = false;
                return;
            }

            _logWriter = TextWriter.Synchronized(new StreamWriter(fileStream, Utility.UTF8NoBom));
            _flushTimer = new(_ => _logWriter?.Flush(), null, 2000, 2000);
        }

        public void Dispose()
        {
            _flushTimer?.Dispose();
            _logWriter?.Flush();
            _logWriter?.Dispose();
        }

        public void LogEvent(object sender, LogEventArgs eventArgs)
        {
            _logWriter.WriteLine(eventArgs.ToString());
        }
    }
}