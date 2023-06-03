namespace UniTAS.Patcher.Interfaces.TASRenderer;

/// <summary>
/// A renderer that processes something in the game to render
/// </summary>
public abstract class Renderer
{
    protected bool Recording { get; private set; }

    public virtual void Start()
    {
        Recording = true;
    }

    public virtual void Stop()
    {
        Recording = false;
    }

    public virtual void Update()
    {
    }

    public abstract bool Available { get; }
}