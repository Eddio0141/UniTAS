using UniTAS.Patcher.Interfaces.Events.Movie;

namespace UniTAS.Patcher.Interfaces.VirtualEnvironment;

/// <summary>
/// Provides a way to create a device that uses legacy input system, or is also used in the new input system.
/// </summary>
public abstract class LegacyInputSystemDevice : InputState, IOnMovieUpdate
{
    /// <summary>
    /// Function invoked when a frame advances.
    /// </summary>
    protected abstract void Update();

    /// <summary>
    /// This method is called before Update and FixedUpdate is called.
    /// Use this to flush buffered inputs.
    /// </summary>
    protected abstract void FlushBufferedInputs();

    public void MovieUpdate(bool fixedUpdate)
    {
        if (!fixedUpdate)
        {
            Update();
        }

        // this just needs to be flushed before legacy input system input is read so its fine to be pre-updates
        FlushBufferedInputs();
    }
}