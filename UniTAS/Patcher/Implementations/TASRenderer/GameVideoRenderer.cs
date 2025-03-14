using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.TASRenderer;
using UniTAS.Patcher.ManualServices;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.TASRenderer;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[ExcludeRegisterIfTesting]
public class GameVideoRenderer : VideoRenderer
{
    private readonly Process _ffmpeg;
    private Texture2D _texture2D;
    private bool _initialSkipVideoTimeLeft;
    private float _renderTimeLeft;
    private Thread _videoProcessingThread;

    private readonly Queue<Color32[]> _videoProcessingQueue = new();
    private readonly Queue<int> _videoProcessingQueueWidth = new();
    private readonly Queue<int> _videoProcessingQueueHeight = new();

    private readonly ILogger _logger;

    public override bool Available { get; } = true;

    public GameVideoRenderer(ILogger logger, IFfmpegProcessFactory ffmpegProcessFactory)
    {
        _logger = logger;

        if (!ffmpegProcessFactory.Available)
        {
            _logger.LogWarning("ffmpeg not available");
            Available = false;
            return;
        }

        _ffmpeg = ffmpegProcessFactory.CreateFfmpegProcess();

        _ffmpeg.ErrorDataReceived += (_, args) =>
        {
            if (args.Data != null)
            {
                _logger.LogDebug($"Video - {args.Data}");
            }
        };

        _ffmpeg.OutputDataReceived += (_, args) =>
        {
            if (args.Data != null)
            {
                _logger.LogDebug($"Video - {args.Data}");
            }
        };
    }

    public override void Start()
    {
        base.Start();

        _texture2D = new(Screen.width, Screen.height, TextureFormat.RGB24, false);

        // ReSharper disable StringLiteralTypo
        var ffmpegArgs =
            $"-y -f rawvideo -vcodec rawvideo -pix_fmt rgb24 -s:v {Width}x{Height} -r {Fps} -i - " +
            $"-an -c:v libx264 -pix_fmt yuv420p -preset ultrafast -crf 16 -vf vflip {OUTPUT_PATH}";
        // ReSharper restore StringLiteralTypo
        _logger.LogDebug($"ffmpeg arguments: {ffmpegArgs}");
        _ffmpeg.StartInfo.Arguments = ffmpegArgs;

        _ffmpeg.Start();
        _ffmpeg.BeginErrorReadLine();
        _ffmpeg.BeginOutputReadLine();

        _initialSkipVideoTimeLeft = true;
        _renderTimeLeft = 0f;

        _videoProcessingQueue.Clear();
        _videoProcessingQueueWidth.Clear();
        _videoProcessingQueueHeight.Clear();

        _videoProcessingThread = new(VideoProcessingThread);
        _videoProcessingThread.Start();

        _logger.LogInfo("Video capture started");
    }

    public override void Stop()
    {
        base.Stop();

        // wait for thread to finish
        _videoProcessingThread.Join();

        _ffmpeg.StandardInput.Close();
        _ffmpeg.WaitForExit();

        // stop stderr and stdout
        _ffmpeg.CancelErrorRead();
        _ffmpeg.CancelOutputRead();

        if (_ffmpeg.ExitCode != 0)
        {
            _logger.LogError("ffmpeg exited with non-zero exit code for video");
        }
    }

    public override void Update()
    {
        _renderTimeLeft -= Time.deltaTime;
        if (_renderTimeLeft > 0)
        {
            return;
        }

        using var _ = Bench.Measure();

        if (_initialSkipVideoTimeLeft)
        {
            _initialSkipVideoTimeLeft = false;
            _renderTimeLeft = 0f;
        }

        var width = Screen.width;
        var height = Screen.height;
        if (_texture2D.width != width || _texture2D.height != height)
        {
            _texture2D.Resize(width, height);
        }

        _texture2D.ReadPixels(new(0, 0, width, height), 0, 0);
        var pixels = _texture2D.GetPixels32();

        // make up for lost time
        if (_renderTimeLeft < 0)
        {
            var framesCountRaw = -_renderTimeLeft / RecordFrameTime;
            var framesCount = (int)framesCountRaw;

            for (var i = 0; i < framesCount; i++)
            {
                PushQueue(pixels, width, height);
            }

            // add any left frames
            _renderTimeLeft = (framesCountRaw - framesCount) * -RecordFrameTime;
        }

        PushQueue(pixels, width, height);

        _renderTimeLeft += RecordFrameTime;
    }

    private void PushQueue(Color32[] pixels, int width, int height)
    {
        _videoProcessingQueue.Enqueue(pixels);
        _videoProcessingQueueWidth.Enqueue(width);
        _videoProcessingQueueHeight.Enqueue(height);
    }

    private void VideoProcessingThread()
    {
        while (Recording || _videoProcessingQueue.Count > 0)
        {
            if (_videoProcessingQueue.Count == 0)
            {
                Thread.Sleep(1);
                continue;
            }

            var pixels = _videoProcessingQueue.Dequeue();
            var width = _videoProcessingQueueWidth.Dequeue();
            var height = _videoProcessingQueueHeight.Dequeue();

            var bytes = new byte[pixels.Length * 3];

            for (var i = 0; i < pixels.Length; i++)
            {
                var color = pixels[i];
                bytes[i * 3] = color.r;
                bytes[i * 3 + 1] = color.g;
                bytes[i * 3 + 2] = color.b;
            }

            // resize
            if (pixels.Length != Width * Height)
            {
                bytes = ImageOperation.Resize(bytes, width, height, Width, Height);
            }

            _ffmpeg.StandardInput.BaseStream.Write(bytes, 0, bytes.Length);
        }

        _ffmpeg.StandardInput.BaseStream.Flush();
        _logger.LogDebug("Video processing thread finished");
    }
}