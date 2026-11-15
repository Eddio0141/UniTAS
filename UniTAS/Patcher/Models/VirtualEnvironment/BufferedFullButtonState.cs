using System;
using System.Collections.Generic;

namespace UniTAS.Patcher.Models.VirtualEnvironment;

/// <summary>
/// Class to handle key state for tracking additional down / up states.
/// </summary>
public class BufferedFullKeyState<TKey>
    where TKey : IEquatable<TKey>
{
    public HashSet<TKey> Held { get; } = [];
    public HashSet<TKey> Down { get; } = [];
    public HashSet<TKey> Up { get; } = [];

    private readonly HashSet<TKey> _bufferedPress = [];
    private readonly HashSet<TKey> _bufferedRelease = [];

    public void ResetState()
    {
        Held.Clear();
        Down.Clear();
        Up.Clear();
        _bufferedPress.Clear();
        _bufferedRelease.Clear();
    }

    /// <summary>
    /// Flushes buffered inputs before frame, sets actual key state.
    /// </summary>
    public void Update()
    {
        Down.Clear();
        Up.Clear();

        foreach (var key in _bufferedPress)
        {
            if (Held.Contains(key)) continue;
            Down.Add(key);
            Held.Add(key);
        }

        foreach (var bufferedRelease in _bufferedRelease)
        {
            Held.RemoveWhere(key =>
            {
                if (!key.Equals(bufferedRelease)) return false;
                Up.Add(key);
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

        if (Held.Contains(key))
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
        foreach (var key in Held)
        {
            _bufferedRelease.Add(key);
        }
    }

    /// <summary>
    /// Checks if the key is pressed this frame.
    /// </summary>
    public bool IsDown(TKey key)
    {
        return Down.Contains(key);
    }

    /// <summary>
    /// Checks if a key was released this frame.
    /// </summary>
    public bool IsUp(TKey key)
    {
        return Up.Contains(key);
    }

    /// <summary>
    /// Checks if a key is held.
    /// </summary>
    public bool IsHeld(TKey key)
    {
        return Held.Contains(key);
    }
}
