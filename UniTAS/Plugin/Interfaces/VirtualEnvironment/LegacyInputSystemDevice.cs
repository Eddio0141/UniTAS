using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.DontRunIfPaused;

namespace UniTAS.Plugin.Interfaces.VirtualEnvironment;

/// <summary>
/// Provides a way to create a device that uses legacy input system, or is also used in the new input system.
/// </summary>
public abstract class LegacyInputSystemDevice : InputState, IOnLastUpdateActual, IOnPreUpdatesActual
{
    protected abstract void Update();

    /// <summary>
    /// This method is called before Update and FixedUpdate is called.
    /// Use this to flush buffered inputs.
    /// </summary>
    protected abstract void FlushBufferedInputs();

    public void OnLastUpdateActual()
    {
        Update();
    }

    public void PreUpdateActual()
    {
        // this just needs to be flushed before legacy input system input is read so its fine to be pre-updates
        FlushBufferedInputs();
    }
}