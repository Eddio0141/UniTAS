using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces.Update;
using UniTAS.Plugin.Logger;
using UnityEngine;

namespace UniTAS.Plugin.GameVideoRender;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class GameRender : IGameRender, IOnLastUpdate
{
    private readonly Process _ffmpeg;
    private const string FfmpegPath = "ffmpeg.exe";

    private const int Fps = 60;
    private const int Width = 1920;
    private const int Height = 1080;
    private const string OutputPath = "output.mp4";

    private Texture2D _texture2D;
    private byte[] _bytes;
    private Color32[] _colors;
    private int _totalBytes;

    private bool _isRecording;

    private readonly ILogger _logger;

    public GameRender(ILogger logger)
    {
        _logger = logger;
        if (!System.IO.File.Exists(FfmpegPath))
        {
            // TODO log error
            throw new("ffmpeg not found");
        }

        _ffmpeg = new();
        _ffmpeg.StartInfo.FileName = FfmpegPath;
        // ffmpeg gets fed raw video data from unity
        // game fps could be variable, but output fps is fixed
        // ffmpeg itself gets fed in png format
        //$"-y -f rawvideo -c:v rawvideo -pix_fmt rgb24 -s:v {Width}x{Height} -r {Fps} -i - {OutputPath}";
        _ffmpeg.StartInfo.Arguments =
            $"-y -f rawvideo -vcodec rawvideo -pix_fmt rgba -s {Width}x{Height} -r {Fps} -i - -threads 0 -preset ultrafast -pix_fmt yuv420p -crf 0 -vf vflip {OutputPath}";
        _ffmpeg.StartInfo.UseShellExecute = false;
        _ffmpeg.StartInfo.RedirectStandardInput = true;
        _ffmpeg.StartInfo.RedirectStandardOutput = true;
        _ffmpeg.StartInfo.RedirectStandardError = true;

        // _ffmpeg.ErrorDataReceived += (_, args) =>
        // {
        //     if (args.Data != null)
        //     {
        //         _logger.LogError(args.Data);
        //     }
        // };
        //
        // _ffmpeg.OutputDataReceived += (_, args) =>
        // {
        //     if (args.Data != null)
        //     {
        //         _logger.LogDebug(args.Data);
        //     }
        // };
    }

    public void Start()
    {
        _logger.LogDebug("Setting up recording");
        // TODO let it able to change resolution
        _texture2D = new(Width, Height, TextureFormat.RGB24, false);
        _totalBytes = Width * Height * 4;
        _bytes = new byte[_totalBytes];

        _ffmpeg.Start();
        _isRecording = true;
        _logger.LogDebug("Started recording");
    }

    public void Stop()
    {
        _logger.LogDebug("Stopping recording");
        _isRecording = false;
        _ffmpeg.StandardInput.Close();
        _ffmpeg.WaitForExit();

        // TODO log error if exit code is not 0

        _logger.LogDebug("Successfully stopped recording");
    }

    public void OnLastUpdate()
    {
        if (!_isRecording) return;

        _texture2D.ReadPixels(new(0, 0, Width, Height), 0, 0);

        _colors = _texture2D.GetPixels32();
        for (var i = 0; i < _colors.Length; i++)
        {
            var color = _colors[i];
            _bytes[i * 4] = color.r;
            _bytes[i * 4 + 1] = color.g;
            _bytes[i * 4 + 2] = color.b;
        }

        _ffmpeg.StandardInput.BaseStream.Write(_bytes, 0, _totalBytes);
        _ffmpeg.StandardInput.BaseStream.Flush();
    }
}