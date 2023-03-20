using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.Logging;

namespace UniTAS.Plugin.Implementations;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class FfmpegProcessFactory : IFfmpegProcessFactory
{
    public bool Available { get; private set; }

    private readonly string[] _ffmpegChecks = { "ffmpeg.exe", "ffmpeg" };
    private string _ffmpegPath;

    public Process CreateFfmpegProcess()
    {
        return Available
            ? new Process
            {
                StartInfo =
                {
                    FileName = _ffmpegPath,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            }
            : null;
    }

    private readonly ILogger _logger;

    public FfmpegProcessFactory(ILogger logger)
    {
        _logger = logger;
        CheckFfmpeg();
    }

    private void CheckFfmpeg()
    {
        string path = null;
        foreach (var ffmpegCheck in _ffmpegChecks)
        {
            Trace.Write($"checking ffmpeg at path: {ffmpegCheck}");
            if (TryRunning(ffmpegCheck))
            {
                path = ffmpegCheck;
                break;
            }

            if (BepInEx.Paths.GameRootPath == null) continue;

            var gamePath = Path.Combine(BepInEx.Paths.GameRootPath, ffmpegCheck);
            Trace.Write($"checking ffmpeg at path: {gamePath}");
            if (TryRunning(gamePath))
            {
                path = gamePath;
                break;
            }
        }

        if (path == null)
        {
            return;
        }

        _ffmpegPath = path;
        Available = true;
        _logger.LogInfo($"Ffmpeg found at {_ffmpegPath}");
    }

    private static bool TryRunning(string path)
    {
        // try running
        var ffmpeg = new Process();
        ffmpeg.StartInfo.FileName = path;
        ffmpeg.StartInfo.UseShellExecute = false;
        ffmpeg.StartInfo.RedirectStandardInput = true;
        ffmpeg.StartInfo.RedirectStandardOutput = true;
        ffmpeg.StartInfo.RedirectStandardError = true;

        try
        {
            ffmpeg.Start();
            ffmpeg.WaitForExit();

            return true;
        }
        catch
        {
            return false;
        }
    }
}