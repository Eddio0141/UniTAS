using System;
using System.Collections.Generic;
using UniTAS.Patcher.Extensions;

namespace UniTAS.Patcher.Interfaces.VirtualEnvironment;

/// <summary>
/// Provides a way to create a device that uses legacy input system, or is also used in the new input system.
/// This contains button states which can be pressed, held, or released.
/// </summary>
public abstract class LegacyInputSystemButtonBasedDevice<TButton> : LegacyInputSystemDevice
    where TButton : IEquatable<TButton>
{
    private readonly List<TButton> _buttons = new();
    private readonly List<TButton> _buttonsDown = new();
    private readonly List<TButton> _buttonsUp = new();

    private readonly List<TButton> _bufferedPressButtons = new();
    private readonly List<TButton> _bufferedReleaseButtons = new();

    protected override void ResetState()
    {
        _buttons.Clear();
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
            if (_buttons.Contains(button)) continue;
            _buttonsDown.Add(button);
            _buttons.Add(button);
        }

        foreach (var bufferedRelease in _bufferedReleaseButtons)
        {
            for (var i = 0; i < _buttons.Count; i++)
            {
                var button = _buttons[i];
                if (!button.Equals(bufferedRelease)) continue;
                _buttonsUp.Add(button);
                _buttons.RemoveAt(i);
                i--;
            }
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
        _bufferedReleaseButtons.RemoveAllEquals(button);
    }

    /// <summary>
    /// Call this method to release a button.
    /// </summary>
    /// <param name="button">The button that is released.</param>
    protected void Release(TButton button)
    {
        _bufferedPressButtons.RemoveAllEquals(button);

        if (_buttons.Contains(button))
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
        _bufferedReleaseButtons.AddRange(_buttons);
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
        return _buttons.Contains(button);
    }

    protected bool AnyButtonHeld => _buttons.Count > 0;
    protected bool AnyButtonDown => _buttonsDown.Count > 0;
}