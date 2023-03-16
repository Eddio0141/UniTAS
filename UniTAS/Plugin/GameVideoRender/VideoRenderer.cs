namespace UniTAS.Plugin.GameVideoRender;

public abstract class VideoRenderer : Renderer
{
    public const string OutputPath = "temp.mp4";

    private int _fps = 60;
    protected float RecordFrameTime { get; private set; } = 1f / 60f;

    public int Fps
    {
        get => _fps;
        set
        {
            RecordFrameTime = 1f / value;
            _fps = value;
        }
    }
}