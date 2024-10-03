namespace UniTAS.Patcher.Interfaces.VirtualEnvironment;

/// <summary>
/// Provides a way to create a device that uses legacy input system, or is also used in the new input system.
/// </summary>
public abstract class LegacyInputSystemDevice : InputState
{
    /// <summary>
    /// Function invoked when a frame advances.
    /// </summary>
    protected abstract override void Update();

    /// <summary>
    /// This method is called before Update and FixedUpdate is called.
    /// Use this to flush buffered inputs.
    /// </summary>
    protected abstract void FlushBufferedInputs();

    public override void MovieUpdate(bool fixedUpdate)
    {
        base.MovieUpdate(fixedUpdate);

        if (fixedUpdate) return;
        
        // this just needs to be flushed before legacy input system input is read so its fine to be pre-updates
        FlushBufferedInputs();
    }
}
