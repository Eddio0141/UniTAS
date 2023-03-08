using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using UniTAS.Plugin.Interfaces.Update;
using UniTAS.Plugin.Logger;
using UniTAS.Plugin.Utils;
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

    private bool _isRecording;
    private bool _initialSkipTimeLeft;

    private readonly ILogger _logger;

    private float _timeLeft;

    private int _fps = 60;

#if TRACE
    private long _avgTicks;
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

    private int _waitingThreads;

    public GameRender(ILogger logger)
    {
        _logger = logger;

        _ffmpeg = new();
        // TODO check if ffmpeg is installed
        _ffmpeg.StartInfo.FileName = "ffmpeg";
        // ffmpeg gets fed raw video data from unity
        _ffmpeg.StartInfo.Arguments =
            $"-y -f rawvideo -vcodec rawvideo -pix_fmt rgb24 -s:v {_width}x{_height} -r {Fps} -i - -an -c:v libx264 -pix_fmt yuv420p -preset ultrafast -crf 16 -vf vflip {OutputPath}";
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
    }

    public void Start()
    {
        if (_isRecording) return;

        _logger.LogDebug("Setting up recording");
        // TODO let it able to change resolution
        _texture2D = new(_width, _height, TextureFormat.RGB24, false);

        _ffmpeg.Start();
        _ffmpeg.BeginErrorReadLine();
        _ffmpeg.BeginOutputReadLine();

        _isRecording = true;
        _initialSkipTimeLeft = true;
        _timeLeft = 0f;
        _waitingThreads = 0;

#if TRACE
        _avgTicks = 0;
        _measurements = 0;
#endif

        StartAudioCapture();

        _logger.LogDebug("Started recording");
    }

    public void Stop()
    {
        if (!_isRecording) return;

        _logger.LogDebug("Stopping recording");
        _isRecording = false;

        // wait for all threads to finish
        _logger.LogDebug($"Waiting for {_waitingThreads} threads to finish");
        while (_waitingThreads > 0)
        {
            Thread.Sleep(10);
        }

        _ffmpeg.StandardInput.Close();
        _ffmpeg.WaitForExit();

        // stop stderr and stdout
        _ffmpeg.CancelErrorRead();
        _ffmpeg.CancelOutputRead();

        SaveWavFile();

#if TRACE
        var avgTicks = _avgTicks / _measurements;
        Trace.Write($"Average ticks: {_avgTicks}, ms: {avgTicks / (float)Stopwatch.Frequency * 1000f}");
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

    // TODO run a thread in the background while recording to process video data
    public void OnLastUpdate()
    {
        if (!_isRecording) return;
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
        // make up for lost time
        var count = 1;
        if (_timeLeft < 0)
        {
            var framesCountRaw = -_timeLeft / _recordFrameTime;
            count += (int)framesCountRaw;

            // add any left frames
            _timeLeft = (framesCountRaw - count - 1) * _recordFrameTime * -1f;
        }

        ThreadPool.QueueUserWorkItem(ProcessVideoData, Tuple.New(_texture2D.GetPixels32(), count));
        _waitingThreads++;

#if TRACE
        sw.Stop();
        Trace.Write($"Elapsed ticks: {sw.ElapsedTicks}, ms: {sw.ElapsedMilliseconds}");
        // detect if we are running too slow
        if (sw.ElapsedMilliseconds > 40)
        {
            Trace.Write($"Slow frame, ignoring measurement");
        }
        else
        {
            _avgTicks += sw.ElapsedTicks;
            _measurements++;
        }
#endif

        _timeLeft += _recordFrameTime;
    }

    private void ProcessVideoData(object data)
    {
        var tuple = (Tuple<Color32[], int>)data;
        var pixels = tuple.Item1;
        var count = tuple.Item2;

        var len = pixels.Length * 3;
        var bytes = new byte[len];

        for (var i = 0; i < pixels.Length; i++)
        {
            var color = pixels[i];
            bytes[i * 3] = color.r;
            bytes[i * 3 + 1] = color.g;
            bytes[i * 3 + 2] = color.b;
        }

        lock (_ffmpeg)
        {
            for (var i = 0; i < count; i++)
            {
                _ffmpeg.StandardInput.BaseStream.Write(bytes, 0, len);
            }
        }

        _waitingThreads--;
    }
}