using UnityEngine;

namespace UniTAS.Plugin.GameVideoRender;

public abstract class VideoRenderer : Renderer
{
    public const string OutputPath = "temp.mp4";

    private const int DefaultFps = 60;
    private int _fps = DefaultFps;
    protected float RecordFrameTime { get; private set; } = 1f / DefaultFps;

    public int Fps
    {
        get => _fps;
        set
        {
            RecordFrameTime = 1f / value;
            _fps = value;
        }
    }

    public int Width { get; set; } = Screen.width;
    public int Height { get; set; } = Screen.height;
}