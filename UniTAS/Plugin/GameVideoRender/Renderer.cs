namespace UniTAS.Plugin.GameVideoRender;

/// <summary>
/// A renderer that processes something in the game to render
/// </summary>
public abstract class Renderer
{
    protected bool Recording { get; private set; }

    protected virtual void Start()
    {
        Recording = true;
    }

    protected virtual void Stop()
    {
        Recording = false;
    }

    protected abstract void Update();

    public abstract bool Available { get; }
}