using System.IO;
using System.Text;
using BepInEx;
using BepInEx.Logging;

namespace UniTAS.Patcher.Utils;

public static class LoggingUtils
{
    public static void InitDiskLogger()
    {
        Logger.Listeners.Add(new DiskLogger());
    }

    private class DiskLogger : ILogListener
    {
        private readonly FileStream _fileStream =
            new(Path.Combine(Paths.BepInExRootPath, "UniTAS.log"), FileMode.Create);

        public void Dispose()
        {
            _fileStream.Dispose();
        }

        public void LogEvent(object sender, LogEventArgs eventArgs)
        {
            var logData = eventArgs.ToStringLine();
            var logBytes = Encoding.UTF8.GetBytes(logData);
            _fileStream.Write(logBytes, 0, logBytes.Length);
        }
    }
}