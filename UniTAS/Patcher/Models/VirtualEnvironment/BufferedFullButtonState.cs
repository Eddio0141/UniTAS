using System;
using System.Collections.Generic;

namespace UniTAS.Patcher.Models.VirtualEnvironment;

/// <summary>
/// Class to handle key state for tracking additional down / up states.
/// </summary>
public class BufferedFullKeyState<TKey>
    where TKey : IEquatable<TKey>
{
    public IReadOnlyCollection<TKey> Held => (IReadOnlyCollection<TKey>)_held;
    public IReadOnlyCollection<TKey> Down => (IReadOnlyCollection<TKey>)_down;
    public IReadOnlyCollection<TKey> Up => (IReadOnlyCollection<TKey>)_up;
    private readonly HashSet<TKey> _held = [];
    private readonly HashSet<TKey> _down = [];
    private readonly HashSet<TKey> _up = [];

    private readonly HashSet<TKey> _bufferedPress = [];
    private readonly HashSet<TKey> _bufferedRelease = [];

    public void ResetState()
    {
        _held.Clear();
        _down.Clear();
        _up.Clear();
        _bufferedPress.Clear();
        _bufferedRelease.Clear();
    }

    /// <summary>
    /// Flushes buffered inputs before frame, sets actual key state.
    /// </summary>
    public void Update()
    {
        _down.Clear();
        _up.Clear();

        foreach (var key in _bufferedPress)
        {
            if (_held.Contains(key)) continue;
            _down.Add(key);
            _held.Add(key);
        }

        foreach (var bufferedRelease in _bufferedRelease)
        {
            _held.RemoveWhere(key =>
            {
                if (!key.Equals(bufferedRelease)) return false;
                _up.Add(key);
                return true;
            });
        }

        _bufferedPress.Clear();
        _bufferedRelease.Clear();
    }

    /// <summary>
    /// Holds a key.
    /// </summary>
    public void Hold(TKey key)
    {
        if (_bufferedPress.Contains(key)) return;
        _bufferedPress.Add(key);
        _bufferedRelease.RemoveWhere(key.Equals);
    }

    /// <summary>
    /// Releases a key.
    /// </summary>
    public void Release(TKey key)
    {
        _bufferedPress.RemoveWhere(key.Equals);

        if (_held.Contains(key))
        {
            _bufferedRelease.Add(key);
        }
    }

    /// <summary>
    /// Releases all key.
    /// </summary>
    public void Clear()
    {
        _bufferedPress.Clear();
        foreach (var key in _held)
        {
            _bufferedRelease.Add(key);
        }
    }

    /// <summary>
    /// Checks if the key is pressed this frame.
    /// </summary>
    public bool IsDown(TKey key)
    {
        return _down.Contains(key);
    }

    /// <summary>
    /// Checks if a key was released this frame.
    /// </summary>
    public bool IsUp(TKey key)
    {
        return _up.Contains(key);
    }

    /// <summary>
    /// Checks if a key is held.
    /// </summary>
    public bool IsHeld(TKey key)
    {
        return _held.Contains(key);
    }
}
