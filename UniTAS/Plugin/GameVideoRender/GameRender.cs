using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using UniTAS.Plugin.Interfaces.Update;
using UniTAS.Plugin.Logger;

namespace UniTAS.Plugin.GameVideoRender;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class GameRender : IGameRender, IOnLastUpdate
{
    private bool _recording;
    private readonly ILogger _logger;

    public int Fps
    {
        set => _videoRenderer.Fps = value;
    }

    public int Width
    {
        set => _videoRenderer.Width = value;
    }

    public int Height
    {
        set => _videoRenderer.Height = value;
    }

    private readonly VideoRenderer _videoRenderer;
    private readonly Renderer[] _renderers;

    private readonly Process _ffmpegMergeVideoAudio;

    public GameRender(ILogger logger, IEnumerable<VideoRenderer> videoRenderers,
        IEnumerable<AudioRenderer> audioRenderers, IFfmpegRunner ffmpegRunner)
    {
        _logger = logger;

        _videoRenderer = videoRenderers.FirstOrDefault(x => x.Available);
        var audioRenderer = audioRenderers.FirstOrDefault(x => x.Available);

        if (_videoRenderer == null)
        {
            _logger.LogError("No video renderer available");
            return;
        }

        if (audioRenderer == null)
        {
            _logger.LogError("No audio renderer available");
            return;
        }

        if (!ffmpegRunner.Available)
        {
            _logger.LogError("ffmpeg not available");
            return;
        }

        _renderers = new Renderer[] { _videoRenderer, audioRenderer };

        _ffmpegMergeVideoAudio = ffmpegRunner.FfmpegProcess;

        _ffmpegMergeVideoAudio.StartInfo.Arguments =
            $"-y -i {VideoRenderer.OutputPath} -i {AudioRenderer.OutputPath} -c:v copy -c:a aac output.mp4";

        _ffmpegMergeVideoAudio.ErrorDataReceived += (_, args) =>
        {
            if (args.Data != null)
            {
                _logger.LogDebug($"Audio video merge - {args.Data}");
            }
        };

        _ffmpegMergeVideoAudio.OutputDataReceived += (_, args) =>
        {
            if (args.Data != null)
            {
                _logger.LogDebug($"Audio video merge - {args.Data}");
            }
        };
    }

    public void Start()
    {
        if (_recording || _renderers == null) return;
        _recording = true;

        _logger.LogInfo("Starting recording");

        foreach (var renderer in _renderers)
        {
            renderer.Start();
        }

        _logger.LogInfo("Started recording");
    }

    public void Stop()
    {
        if (!_recording) return;
        _recording = false;

        _logger.LogInfo("Stopping recording");

        foreach (var renderer in _renderers)
        {
            renderer.Stop();
        }

        _logger.LogDebug("Merging audio and video");

        // start merge
        _ffmpegMergeVideoAudio.Start();
        _ffmpegMergeVideoAudio.BeginErrorReadLine();
        _ffmpegMergeVideoAudio.BeginOutputReadLine();

        _ffmpegMergeVideoAudio.WaitForExit();

        // stop stderr and stdout
        _ffmpegMergeVideoAudio.CancelErrorRead();
        _ffmpegMergeVideoAudio.CancelOutputRead();

        if (_ffmpegMergeVideoAudio.ExitCode != 0)
        {
            _logger.LogError("ffmpeg exited with non-zero exit code, merge failed");
            return;
        }

        try
        {
            // delete video file
            File.Delete(VideoRenderer.OutputPath);
        }
        catch (Exception e)
        {
            Trace.Write("Exception trying to delete video file, ignoring: " + e);
        }

        try
        {
            // delete audio file
            File.Delete(AudioRenderer.OutputPath);
        }
        catch (Exception e)
        {
            Trace.Write("Exception trying to delete audio file, ignoring: " + e);
        }

        _logger.LogInfo("Successfully stopped recording");
    }

    public void OnLastUpdate()
    {
        if (!_recording) return;

        foreach (var renderer in _renderers)
        {
            renderer.Update();
        }
    }
}