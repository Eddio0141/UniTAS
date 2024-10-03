using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.DontRunIfPaused;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Interfaces.TASRenderer;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Singleton]
public class GameRender : IGameRender, IOnLastUpdateActual
{
    private bool _recording;

    private readonly ILogger _logger;
    private readonly IMovieLogger _movieLogger;

    public int Fps
    {
        set
        {
            if (_videoRenderer == null) return;
            _videoRenderer.Fps = value;
        }
    }

    public int Width
    {
        set
        {
            if (_videoRenderer == null) return;
            _videoRenderer.Width = value;
        }
    }

    public int Height
    {
        set
        {
            if (_videoRenderer == null) return;
            _videoRenderer.Height = value;
        }
    }

    public string VideoPath { get; set; } = "output.mp4";

    private readonly VideoRenderer _videoRenderer;
    private readonly bool _hasVideoRenderer;
    private readonly bool _hasAudioRenderer;
    private readonly Renderer[] _renderers;

    private readonly Process _ffmpegMergeVideoAudio;

    public GameRender(ILogger logger, IEnumerable<VideoRenderer> videoRenderers,
        IEnumerable<AudioRenderer> audioRenderers, IFfmpegProcessFactory ffmpegProcessFactory, IMovieLogger movieLogger)
    {
        _logger = logger;
        _movieLogger = movieLogger;

        _videoRenderer = videoRenderers.FirstOrDefault(x => x.Available);
        var audioRenderer = audioRenderers.FirstOrDefault(x => x.Available);

        if (_videoRenderer == null)
        {
            _logger.LogWarning("No video renderer available");
            return;
        }

        _hasVideoRenderer = true;

        if (audioRenderer == null)
        {
            _logger.LogWarning("No audio renderer available");
            return;
        }

        _hasAudioRenderer = true;

        if (!ffmpegProcessFactory.Available)
        {
            _logger.LogWarning("ffmpeg not available");
            return;
        }

        _renderers = [_videoRenderer, audioRenderer];

        _ffmpegMergeVideoAudio = ffmpegProcessFactory.CreateFfmpegProcess();

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
        if (_recording) return;
        if (!_hasVideoRenderer)
        {
            _movieLogger.LogWarning("No video renderer available, cannot start recording", true);
            return;
        }

        if (!_hasAudioRenderer)
        {
            _movieLogger.LogWarning("No audio recorder available, cannot start recording", true);
            return;
        }

        _recording = true;

        _logger.LogInfo("Starting recording");

        foreach (var renderer in _renderers)
        {
            renderer.Start();
        }

        _logger.LogInfo("Started recording");
        _movieLogger.LogInfo("Started recording", true);
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
        _ffmpegMergeVideoAudio.StartInfo.Arguments =
            $"-y -i {VideoRenderer.OUTPUT_PATH} -i {AudioRenderer.OUTPUT_PATH} -c:v copy -c:a aac {VideoPath}";

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
            _movieLogger.LogError("something went wrong merging audio and video, check logs", true);
            return;
        }

        try
        {
            // delete video file
            File.Delete(VideoRenderer.OUTPUT_PATH);
        }
        catch (Exception e)
        {
            _logger.LogDebug("Exception trying to delete video file, ignoring: " + e);
        }

        try
        {
            // delete audio file
            File.Delete(AudioRenderer.OUTPUT_PATH);
        }
        catch (Exception e)
        {
            _logger.LogDebug("Exception trying to delete audio file, ignoring: " + e);
        }

        _logger.LogInfo("Successfully stopped recording");
        _movieLogger.LogInfo("Successfully stopped recording", true);
    }

    public void OnLastUpdateActual()
    {
        if (!_recording) return;

        foreach (var renderer in _renderers)
        {
            renderer.Update();
        }
    }
}