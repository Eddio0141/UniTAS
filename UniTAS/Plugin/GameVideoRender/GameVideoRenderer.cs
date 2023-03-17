using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using UniTAS.Plugin.Logger;
using UnityEngine;

namespace UniTAS.Plugin.GameVideoRender;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class GameVideoRenderer : VideoRenderer
{
    private readonly Process _ffmpeg;
    private Texture2D _texture2D;
    private bool _initialSkipVideoTimeLeft;
    private float _renderTimeLeft;
    private Thread _videoProcessingThread;

    private Queue<Color32[]> _videoProcessingQueue;
    private Queue<int> _videoProcessingQueueWidth;
    private Queue<int> _videoProcessingQueueHeight;

    private readonly ILogger _logger;

#if TRACE
    private long _avgTicks;
    private long _totalTicks;
    private int _measurements;
#endif

    public override bool Available { get; } = true;

    public GameVideoRenderer(ILogger logger, IFfmpegRunner ffmpegRunner)
    {
        _logger = logger;

        if (!ffmpegRunner.Available)
        {
            _logger.LogError("ffmpeg not available");
            Available = false;
            return;
        }

        _ffmpeg = ffmpegRunner.FfmpegProcess;
        // ReSharper disable StringLiteralTypo
        var ffmpegArgs =
            $"-y -f rawvideo -vcodec rawvideo -pix_fmt rgb24 -s:v {Width}x{Height} -r {Fps} -i - " +
            $"-an -c:v libx264 -pix_fmt yuv420p -preset ultrafast -crf 16 -vf vflip {OutputPath}";
        // ReSharper restore StringLiteralTypo
        _logger.LogDebug($"ffmpeg arguments: {ffmpegArgs}");
        _ffmpeg.StartInfo.Arguments = ffmpegArgs;

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

        _texture2D = new(Width, Height, TextureFormat.RGB24, false);

        _ffmpeg.Start();
        _ffmpeg.BeginErrorReadLine();
        _ffmpeg.BeginOutputReadLine();

        _initialSkipVideoTimeLeft = true;
        _renderTimeLeft = 0f;

        _videoProcessingQueue = new();
        _videoProcessingQueueWidth = new();
        _videoProcessingQueueHeight = new();

        _videoProcessingThread = new(VideoProcessingThread);
        _videoProcessingThread.Start();

#if TRACE
        _avgTicks = 0;
        _totalTicks = 0;
        _measurements = 0;
#endif

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

#if TRACE
        if (_measurements == 0)
        {
            Trace.Write("No render avg measurements");
        }
        else
        {
            var avgTicks = _avgTicks / _measurements;
            Trace.Write($"Average ticks: {_avgTicks}, ms: {avgTicks / (float)Stopwatch.Frequency * 1000f}");
            Trace.Write($"Total ticks: {_totalTicks}, ms: {_totalTicks / (float)Stopwatch.Frequency * 1000f}");
        }
#endif

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

        if (_initialSkipVideoTimeLeft)
        {
            _initialSkipVideoTimeLeft = false;
            _renderTimeLeft = 0f;
        }

#if TRACE
        var sw = Stopwatch.StartNew();
#endif

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

#if TRACE
        sw.Stop();
        _avgTicks += sw.ElapsedTicks;
        _totalTicks += sw.ElapsedTicks;
        _measurements++;
#endif

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
        while (Recording)
        {
            if (_videoProcessingQueue.Count == 0)
            {
                Thread.Sleep(1);
                continue;
            }

            var pixels = _videoProcessingQueue.Dequeue();
            var width = _videoProcessingQueueWidth.Dequeue();
            var height = _videoProcessingQueueHeight.Dequeue();

            var len = pixels.Length * 3;
            var bytes = new byte[len];

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
                bytes = Resize(bytes, width, height, Width, Height);
            }

            _ffmpeg.StandardInput.BaseStream.Write(bytes, 0, len);
        }

        _ffmpeg.StandardInput.BaseStream.Flush();
        Trace.Write("Video processing thread finished");
    }

    /// <summary>
    /// Resizes the image to a different resolution while maintaining aspect ratio
    /// Fills in the rest of the image with black
    /// </summary>
    /// <returns>Resized array</returns>
    private static byte[] Resize(byte[] original, int originalWidth, int originalHeight, int targetWidth,
        int targetHeight)
    {
        var resized = new byte[targetWidth * targetHeight * 3];

        var xMove = (float)originalWidth / targetWidth;
        var yMove = (float)originalHeight / targetHeight;

        var startX = targetWidth > originalWidth ? (originalWidth - targetWidth) / 2 : 0;
        var startY = targetHeight > originalHeight ? (originalHeight - targetHeight) / 2 : 0;
        var endX = targetWidth > originalWidth ? startX + originalWidth : targetWidth;
        var endY = targetHeight > originalHeight ? startY + originalHeight : targetHeight;

        for (var y = startY; y < endY; y++)
        {
            for (var x = startX; x < endX; x++)
            {
                var originalX = (int)(x * xMove);
                var originalY = (int)(y * yMove);

                var originalIndex = (originalY * originalWidth + originalX) * 3;
                var targetIndex = (y * targetWidth + x) * 3;

                resized[targetIndex] = original[originalIndex];
                resized[targetIndex + 1] = original[originalIndex + 1];
                resized[targetIndex + 2] = original[originalIndex + 2];
            }
        }

        return resized;
    }
}