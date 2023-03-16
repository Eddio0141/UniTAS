using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Logger;

namespace UniTAS.Plugin.GameVideoRender;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class FfmpegRunner : IFfmpegRunner
{
    public bool Available { get; private set; }

    private readonly string[] _ffmpegChecks = { "ffmpeg.exe", "ffmpeg" };
    private string _ffmpegPath;

    public Process FfmpegProcess => Available
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

    private readonly ILogger _logger;

    public FfmpegRunner(ILogger logger)
    {
        _logger = logger;
        CheckFfmpeg();
    }

    private void CheckFfmpeg()
    {
        foreach (var ffmpegCheck in _ffmpegChecks)
        {
            // try running
            var ffmpeg = new Process();
            ffmpeg.StartInfo.FileName = ffmpegCheck;
            ffmpeg.StartInfo.UseShellExecute = false;
            ffmpeg.StartInfo.RedirectStandardInput = true;
            ffmpeg.StartInfo.RedirectStandardOutput = true;
            ffmpeg.StartInfo.RedirectStandardError = true;

            try
            {
                ffmpeg.Start();
                ffmpeg.WaitForExit();

                // found ffmpeg
                _logger.LogDebug($"Found ffmpeg at {ffmpegCheck}");
                Available = true;
                _ffmpegPath = ffmpegCheck;
                break;
            }
            catch
            {
                // ignored
            }
        }
    }
}