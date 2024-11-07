using System;
using System.Collections.Generic;

namespace UniTAS.Patcher.Interfaces.VirtualEnvironment;

/// <summary>
/// Provides a way to create a device that uses legacy input system, or is also used in the new input system.
/// This contains button states which can be pressed, held, or released.
/// </summary>
public abstract class LegacyInputSystemButtonBasedDevice<TButton> : LegacyInputSystemDevice
    where TButton : IEquatable<TButton>
{
    protected HashSet<TButton> Buttons { get; } = [];
    private readonly HashSet<TButton> _buttonsDown = [];
    private readonly HashSet<TButton> _buttonsUp = [];

    private readonly HashSet<TButton> _bufferedPressButtons = [];
    private readonly HashSet<TButton> _bufferedReleaseButtons = [];

    protected override void ResetState()
    {
        Buttons.Clear();
        _buttonsDown.Clear();
        _buttonsUp.Clear();
        _bufferedPressButtons.Clear();
        _bufferedReleaseButtons.Clear();
    }

    /// <summary>
    /// This method is called before Update and FixedUpdate is called.
    /// Use this to flush buffered inputs.
    /// </summary>
    protected override void FlushBufferedInputs()
    {
        foreach (var button in _bufferedPressButtons)
        {
            if (Buttons.Contains(button)) continue;
            _buttonsDown.Add(button);
            Buttons.Add(button);
        }

        foreach (var bufferedRelease in _bufferedReleaseButtons)
        {
            Buttons.RemoveWhere(button =>
            {
                if (!button.Equals(bufferedRelease)) return false;
                _buttonsUp.Add(button);
                return true;
            });
        }

        _bufferedPressButtons.Clear();
        _bufferedReleaseButtons.Clear();
    }

    /// <summary>
    /// This method is called every frame.
    /// Use this to update the state of the device.
    /// </summary>
    protected override void Update()
    {
        _buttonsDown.Clear();
        _buttonsUp.Clear();
    }

    /// <summary>
    /// Call this method to hold a button.
    /// </summary>
    /// <param name="button">The button that is held.</param>
    protected void Hold(TButton button)
    {
        if (_bufferedPressButtons.Contains(button)) return;
        _bufferedPressButtons.Add(button);
        _bufferedReleaseButtons.RemoveWhere(button.Equals);
    }

    /// <summary>
    /// Call this method to release a button.
    /// </summary>
    /// <param name="button">The button that is released.</param>
    protected void Release(TButton button)
    {
        _bufferedPressButtons.RemoveWhere(button.Equals);

        if (Buttons.Contains(button))
        {
            _bufferedReleaseButtons.Add(button);
        }
    }

    /// <summary>
    /// Releases all buttons.
    /// </summary>
    protected void ReleaseAllButtons()
    {
        _bufferedPressButtons.Clear();
        foreach (var button in Buttons)
        {
            _bufferedReleaseButtons.Add(button);
        }
    }

    /// <summary>
    /// Checks if the button is pressed this frame.
    /// </summary>
    /// <param name="button">The button to check</param>
    /// <returns>If the button is held this frame.</returns>
    protected bool IsButtonDown(TButton button)
    {
        return _buttonsDown.Contains(button);
    }

    /// <summary>
    /// Checks if a button was released this frame.
    /// </summary>
    /// <param name="button">The button to check.</param>
    /// <returns>If the button is released this frame.</returns>
    protected bool IsButtonUp(TButton button)
    {
        return _buttonsUp.Contains(button);
    }

    /// <summary>
    /// Checks if a button is held.
    /// </summary>
    /// <param name="button">Button to check</param>
    /// <returns>If the button is held.</returns>
    protected bool IsButtonHeld(TButton button)
    {
        return Buttons.Contains(button);
    }

    protected bool AnyButtonHeld => Buttons.Count > 0;
    protected bool AnyButtonDown => _buttonsDown.Count > 0;
}