using Core.UnityHooks.InputLegacy;
using System.Collections.Generic;

namespace Core.TAS.Input;

public static class Keyboard
{
    public static List<KeyCodeType> Keys { get; internal set; }
    public static List<KeyCodeType> KeysDown { get; private set; }
    public static List<KeyCodeType> KeysUp { get; private set; }
    private static readonly List<KeyCodeType> KeysPrev;

    static Keyboard()
    {
        Keys = new();
        KeysDown = new();
        KeysUp = new();
        KeysPrev = new();
    }

    public static void Update()
    {
        KeysDown.Clear();
        KeysUp.Clear();

        foreach (var key in KeysPrev)
        {
            if (!Keys.Contains(key))
            {
                KeysUp.Add(key);
                KeysPrev.Remove(key);
            }
        }
        foreach (var key in Keys)
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