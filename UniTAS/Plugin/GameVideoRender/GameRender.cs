using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using UniTAS.Plugin.Interfaces.Update;
using UniTAS.Plugin.Logger;
using UnityEngine;

namespace UniTAS.Plugin.GameVideoRender;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public partial class GameRender : IGameRender, IOnLastUpdate
{
    private readonly Process _ffmpeg;

    private readonly int _width = Screen.width;
    private readonly int _height = Screen.height;
    private const string OutputPath = "output.mp4";

    private Texture2D _texture2D;

    private bool _recording;
    private bool _initialSkipTimeLeft;

    private readonly ILogger _logger;

    private float _timeLeft;
    private double _renderedTime;

    private int _fps = 60;

#if TRACE
    private long _avgTicks;
    private long _totalTicks;
    private int _measurements;
#endif

    public int Fps
    {
        get => _fps;
        set
        {
            _recordFrameTime = 1f / value;
            _fps = value;
        }
    }

    private float _recordFrameTime;

    private Thread _videoProcessingThread;
    private Queue<Texture2D> _videoProcessingQueue;

    public GameRender(ILogger logger)
    {
        _logger = logger;

        _ffmpeg = new();
        // TODO check if ffmpeg is installed
        _ffmpeg.StartInfo.FileName = "ffmpeg";

        // ReSharper disable StringLiteralTypo
        var ffmpegArgs =
            $"-y -f rawvideo -vcodec rawvideo -pix_fmt rgb24 -s:v {_width}x{_height} -r {Fps} -i - " +
            $"-an -c:v libx264 -pix_fmt yuv420p -preset ultrafast -crf 16 -vf vflip {OutputPath}";
        // ReSharper restore StringLiteralTypo
        _logger.LogDebug($"ffmpeg arguments: {ffmpegArgs}");
        _ffmpeg.StartInfo.Arguments = ffmpegArgs;

        _ffmpeg.StartInfo.UseShellExecute = false;
        _ffmpeg.StartInfo.RedirectStandardInput = true;
        _ffmpeg.StartInfo.RedirectStandardOutput = true;
        _ffmpeg.StartInfo.RedirectStandardError = true;

        _ffmpeg.ErrorDataReceived += (_, args) =>
        {
            if (args.Data != null)
            {
                _logger.LogDebug(args.Data);
            }
        };

        _ffmpeg.OutputDataReceived += (_, args) =>
        {
            if (args.Data != null)
            {
                _logger.LogDebug(args.Data);
            }
        };

        _recordFrameTime = 1f / Fps;
    }

    public void Start()
    {
        if (_recording) return;

        _logger.LogDebug("Setting up recording");
        // TODO let it able to change resolution
        _texture2D = new(_width, _height, TextureFormat.RGB24, false);

        _ffmpeg.Start();
        _ffmpeg.BeginErrorReadLine();
        _ffmpeg.BeginOutputReadLine();

        _recording = true;
        _initialSkipTimeLeft = true;
        _timeLeft = 0f;
        _renderedTime = 0f;
        _audioTime = 0f;

        _videoProcessingQueue = new();
        _videoProcessingThread = new(VideoProcessingThread);
        _videoProcessingThread.Start();

#if TRACE
        _avgTicks = 0;
        _totalTicks = 0;
        _measurements = 0;
#endif

        StartAudioCapture();

        _logger.LogDebug("Started recording");
    }

    public void Stop()
    {
        if (!_recording) return;

        _logger.LogDebug("Stopping recording");
        _recording = false;

        // wait for thread to finish
        _videoProcessingThread.Join();

        _ffmpeg.StandardInput.Close();
        _ffmpeg.WaitForExit();

        // stop stderr and stdout
        _ffmpeg.CancelErrorRead();
        _ffmpeg.CancelOutputRead();

        // SaveWavFile();

#if TRACE
        var avgTicks = _avgTicks / _measurements;
        Trace.Write($"Average ticks: {_avgTicks}, ms: {avgTicks / (float)Stopwatch.Frequency * 1000f}");
        Trace.Write($"Total ticks: {_totalTicks}, ms: {_totalTicks / (float)Stopwatch.Frequency * 1000f}");
#endif

        if (_ffmpeg.ExitCode != 0)
        {
            _logger.LogError("ffmpeg exited with non-zero exit code");
            return;
        }

        // merge audio and video
        // TODO clean this up later
        var ffmpeg = new Process();
        ffmpeg.StartInfo.FileName = "ffmpeg";
        ffmpeg.StartInfo.Arguments =
            $"-y -i {OutputPath} -i {OutputFile} -c:v copy -c:a aac -strict experimental {OutputPath}-merged.mp4";

        ffmpeg.StartInfo.UseShellExecute = false;
        ffmpeg.Start();
        ffmpeg.WaitForExit();

        _logger.LogDebug("Successfully stopped recording");
    }

    public void OnLastUpdate()
    {
        if (!_recording) return;
        _timeLeft -= Time.deltaTime;
        if (_timeLeft > 0)
        {
            return;
        }

        if (_initialSkipTimeLeft)
        {
            _initialSkipTimeLeft = false;
            _timeLeft = 0f;
        }

#if TRACE
        var sw = Stopwatch.StartNew();
#endif
        _texture2D.ReadPixels(new(0, 0, _width, _height), 0, 0);

        // make up for lost time
        if (_timeLeft < 0)
        {
            var framesCountRaw = -_timeLeft / _recordFrameTime;
            var framesCount = (int)framesCountRaw;

            for (var i = 0; i < framesCount; i++)
            {
                _videoProcessingQueue.Enqueue(_texture2D);
            }

            // add any left frames
            _timeLeft = (framesCountRaw - framesCount) * -_recordFrameTime;
        }

        _videoProcessingQueue.Enqueue(_texture2D);

#if TRACE
        sw.Stop();
        _avgTicks += sw.ElapsedTicks;
        _totalTicks += sw.ElapsedTicks;
        _measurements++;
#endif

        _timeLeft += _recordFrameTime;
        _renderedTime += _recordFrameTime;
    }

    private void VideoProcessingThread()
    {
        while (true)
        {
            if (!_recording && _videoProcessingQueue.Count == 0) break;

            if (_videoProcessingQueue.Count == 0)
            {
                Thread.Sleep(1);
                continue;
            }

            var texture = _videoProcessingQueue.Dequeue();
            var pixels = texture.GetPixels32();

            var len = pixels.Length * 3;
            var bytes = new byte[len];

            for (var i = 0; i < pixels.Length; i++)
            {
                var color = pixels[i];
                bytes[i * 3] = color.r;
                bytes[i * 3 + 1] = color.g;
                bytes[i * 3 + 2] = color.b;
            }

            _ffmpeg.StandardInput.BaseStream.Write(bytes, 0, len);
        }
    }
}