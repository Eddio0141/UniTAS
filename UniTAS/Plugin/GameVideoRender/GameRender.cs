using System.Diagnostics;
using UniTAS.Plugin.Interfaces.Update;
using UnityEngine;

namespace UniTAS.Plugin.GameVideoRender;

public class GameRender : IOnPostRender, IGameRender
{
    private readonly Process _ffmpeg;
    private const string FfmpegPath = "ffmpeg.exe";

    private const int Fps = 60;
    private const int Width = 1920;
    private const int Height = 1080;
    private const string OutputPath = "output.mp4";

    private RenderTexture _renderTexture;
    private Texture2D _texture2D;

    private bool _isRecording;

    public GameRender()
    {
        if (!System.IO.File.Exists(FfmpegPath))
        {
            // TODO log error
            throw new("ffmpeg not found");
        }

        _ffmpeg = new();
        _ffmpeg.StartInfo.FileName = FfmpegPath;
        // ffmpeg gets fed raw video data from unity
        // game fps could be variable, but output fps is fixed
        _ffmpeg.StartInfo.Arguments =
            $"-y -f rawvideo -vcodec rawvideo -s {Width}x{Height} -pix_fmt bgra -r {Fps} -i - -an -vcodec libx264 -pix_fmt yuv420p -preset ultrafast -crf 0 {OutputPath}";
        _ffmpeg.StartInfo.UseShellExecute = false;
        _ffmpeg.StartInfo.RedirectStandardInput = true;
        _ffmpeg.StartInfo.RedirectStandardOutput = true;
        _ffmpeg.StartInfo.RedirectStandardError = true;
    }

    public void Start()
    {
        // TODO let it able to change resolution
        _renderTexture = new(Width, Height, 24);
        _texture2D = new(Width, Height, TextureFormat.RGBA32, false);
        // TODO find out if to use current camera or main camera
        var camera = Camera.main;
        if (camera == null)
        {
            // TODO log error
            return;
        }

        camera.targetTexture = _renderTexture;

        _ffmpeg.Start();
        _isRecording = true;
    }

    public void Stop()
    {
        _isRecording = false;
        _ffmpeg.StandardInput.Close();
        _ffmpeg.WaitForExit();

        // TODO log error if exit code is not 0

        _renderTexture.Release();
        var camera = Camera.main;
        if (camera != null)
        {
            camera.targetTexture = null;
        }
    }

    public void OnPostRender()
    {
        if (!_isRecording) return;

        RenderTexture.active = _renderTexture;
        _texture2D.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);

        var bytes = _texture2D.EncodeToPNG();

        _ffmpeg.StandardInput.BaseStream.Write(bytes, 0, bytes.Length);
    }
}