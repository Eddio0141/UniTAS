using System.Collections.Generic;
using UnityEngine;

namespace UniTASPlugin.FakeGameState.InputLegacy;

public static class Keyboard
{
    public static List<KeyCode> Keys { get; internal set; } = new();
    public static List<KeyCode> KeysDown { get; private set; } = new();
    public static List<KeyCode> KeysUp { get; private set; } = new();
    private static readonly List<KeyCode> KeysPrev = new();

    public static void Update()
    {
        KeysDown.Clear();
        KeysUp.Clear();

        foreach (KeyCode key in KeysPrev)
        {
            if (!Keys.Contains(key))
            {
                KeysUp.Add(key);
                KeysPrev.Remove(key);
            }
        }
        foreach (KeyCode key in Keys)
        {
            if (!KeysPrev.Contains(key))
            {
                KeysDown.Add(key);
                KeysPrev.Add(key);
            }
        }
    }

    public static void Clear()
    {
        Keys.Clear();
        KeysDown.Clear();
        KeysUp.Clear();
        KeysPrev.Clear();
    }
}