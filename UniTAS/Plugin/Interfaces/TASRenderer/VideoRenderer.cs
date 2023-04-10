using UniTAS.Plugin.Interfaces.DependencyInjection;
using UnityEngine;

namespace UniTAS.Plugin.Interfaces.TASRenderer;

[RegisterAll]
public abstract class VideoRenderer : Renderer
{
    public const string OutputPath = "temp.mp4";

    private const int DEFAULT_FPS = 60;
    private int _fps = DEFAULT_FPS;
    protected float RecordFrameTime { get; private set; } = 1f / DEFAULT_FPS;

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