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
    private byte[] _bytes;
    private Color32[] _colors;
    private int _totalBytes;

    private bool _isRecording;
    private bool _initialSkipTimeLeft;

    private readonly ILogger _logger;

    private float _timeLeft;

    private int _fps = 60;

    private Thread _videoProcessingThread;

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
        _totalBytes = _width * _height * 3;
        _bytes = new byte[_totalBytes];

        _ffmpeg.Start();
        _ffmpeg.BeginErrorReadLine();
        _ffmpeg.BeginOutputReadLine();

        _isRecording = true;
        _initialSkipTimeLeft = true;
        _timeLeft = 0f;

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
        _texture2D.ReadPixels(new(0, 0, _width, _height), 0, 0);
        _colors = _texture2D.GetPixels32();

        for (var i = 0; i < _colors.Length; i++)
        {
            var color = _colors[i];
            _bytes[i * 3] = color.r;
            _bytes[i * 3 + 1] = color.g;
            _bytes[i * 3 + 2] = color.b;
        }

        // make up for lost time
        if (_timeLeft < 0)
        {
            var framesCountRaw = -_timeLeft / _recordFrameTime;
            var framesToSkip = (int)framesCountRaw;


            for (var i = 0; i < framesToSkip; i++)
            {
                _ffmpeg.StandardInput.BaseStream.Write(_bytes, 0, _totalBytes);
            }

            // add any left frames
            _timeLeft = (framesCountRaw - framesToSkip) * _recordFrameTime * -1f;
        }

        _ffmpeg.StandardInput.BaseStream.Write(_bytes, 0, _totalBytes);

#if TRACE
        sw.Stop();
        _avgTicks += sw.ElapsedTicks;
        _measurements++;
#endif

        _timeLeft += _recordFrameTime;
    }
}