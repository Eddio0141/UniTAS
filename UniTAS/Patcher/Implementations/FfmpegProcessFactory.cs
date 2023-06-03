using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Singleton]
public class FfmpegProcessFactory : IFfmpegProcessFactory
{
    public bool Available { get; private set; }

    private readonly string[] _ffmpegChecks = { "ffmpeg.exe", "ffmpeg" };
    private string _ffmpegPath;

    private readonly ILogger _logger;

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
            _logger.LogDebug($"checking ffmpeg at path: {ffmpegCheck}");
            if (TryRunning(ffmpegCheck))
            {
                path = ffmpegCheck;
                break;
            }

            // find in env path and env path for user
            var envVar = Environment.GetEnvironmentVariable("PATH");
            var envVarUser = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);

            if (envVar != null || envVarUser != null)
            {
                var envPaths = $"{envVar}{Path.PathSeparator}{envVarUser}".Split(Path.PathSeparator);

                foreach (var envPath in envPaths)
                {
                    if (string.IsNullOrEmpty(envPath)) continue;

                    var envPathCheck = Path.Combine(envPath.Trim(), ffmpegCheck);
                    _logger.LogDebug($"checking ffmpeg at path: {envPathCheck}");

                    if (TryRunning(envPathCheck))
                    {
                        path = envPathCheck;
                        break;
                    }
                }

                if (path != null) break;
            }

            if (BepInEx.Paths.GameRootPath != null)
            {
                var gamePath = Path.Combine(BepInEx.Paths.GameRootPath, ffmpegCheck);
                _logger.LogDebug($"checking ffmpeg at path: {gamePath}");
                if (TryRunning(gamePath))
                {
                    path = gamePath;
                    break;
                }
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