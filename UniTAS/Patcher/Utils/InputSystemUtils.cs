using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Utils;

public static class InputSystemUtils
{
    private static readonly Dictionary<string, KeyCode> KeyStringToKeyCodeDict = new()
    {
        { "backspace", KeyCode.Backspace },
        { "delete", KeyCode.Delete },
        { "tab", KeyCode.Tab },
        { "clear", KeyCode.Clear },
        { "return", KeyCode.Return },
        { "pause", KeyCode.Pause },
        { "escape", KeyCode.Escape },
        { "space", KeyCode.Space },
        { "equals", KeyCode.Equals },
        { "enter", KeyCode.Return },
        { "up", KeyCode.UpArrow },
        { "down", KeyCode.DownArrow },
        { "right", KeyCode.RightArrow },
        { "left", KeyCode.LeftArrow },
        { "insert", KeyCode.Insert },
        { "home", KeyCode.Home },
        { "end", KeyCode.End },
        { "page up", KeyCode.PageUp },
        { "page down", KeyCode.PageDown },
        { "-", KeyCode.Minus },
        { "=", KeyCode.Equals },
        { "!", KeyCode.Exclaim },
        { "@", KeyCode.At },
        { "#", KeyCode.Hash },
        { "$", KeyCode.Dollar },
        { "^", KeyCode.Caret },
        { "&", KeyCode.Ampersand },
        { "*", KeyCode.Asterisk },
        { "(", KeyCode.LeftParen },
        { ")", KeyCode.RightParen },
        { "_", KeyCode.Underscore },
        { "+", KeyCode.Plus },
        { "[", KeyCode.LeftBracket },
        { "]", KeyCode.RightBracket },
        { "`", KeyCode.BackQuote },
        { ";", KeyCode.Semicolon },
        { "'", KeyCode.Quote },
        { "\\", KeyCode.Backslash },
        { ":", KeyCode.Colon },
        { "\"", KeyCode.DoubleQuote },
        { ",", KeyCode.Comma },
        { ".", KeyCode.Period },
        { "/", KeyCode.Slash },
        { "<", KeyCode.Less },
        { ">", KeyCode.Greater },
        { "?", KeyCode.Question },
        { "numlock", KeyCode.Numlock },
        { "caps lock", KeyCode.CapsLock },
        { "scroll lock", KeyCode.ScrollLock },
        { "right shift", KeyCode.RightShift },
        { "left shift", KeyCode.LeftShift },
        { "right ctrl", KeyCode.RightControl },
        { "left ctrl", KeyCode.LeftControl },
        { "right alt", KeyCode.RightAlt },
        { "left alt", KeyCode.LeftAlt },
        { "right cmd", KeyCode.RightApple },
        { "left cmd", KeyCode.LeftApple },
        { "right super", KeyCode.RightWindows },
        { "left super", KeyCode.LeftWindows },
        { "alt gr", KeyCode.AltGr },
        // TODO what is this
        // { "compose", KeyCode.Compose },
        { "help", KeyCode.Help },
        { "print screen", KeyCode.Print },
        { "sys req", KeyCode.SysReq },
        { "break", KeyCode.Break },
        { "menu", KeyCode.Menu },
        // TODO those
        // { "power", KeyCode.Power },
        // { "euro", KeyCode.Euro },
        // { "undo", KeyCode.Undo },
    };

    private static readonly Dictionary<string, string> KeyStringToKeyCodeEnumsExtras = new()
    {
        { "%", "Percent" },
        { "{", "LeftCurlyBracket" },
        { "}", "RightCurlyBracket" },
        { "~", "Tilde" },
        { "|", "Pipe" },
    };

    public static void KeyStringToKeys(string key, out KeyCode? keyCode, out Key? newKey)
    {
        keyCode = KeyCodeParse(key);
        newKey = NewKeyParse(key);

        if (keyCode.HasValue)
        {
            keyCode = keyCode.Value;
            if (!newKey.HasValue && keyCode.HasValue)
            {
                newKey = NewKeyParse(keyCode.Value);
            }

            return;
        }

        if (newKey.HasValue)
        {
            newKey = newKey.Value;
            if (!keyCode.HasValue && newKey.HasValue)
            {
                keyCode = KeyCodeParse(newKey.Value);
            }
        }
    }

    private static KeyCode? KeyCodeParse(Key key)
    {
        switch (key)
        {
            case Key.None:
                return KeyCode.None;
            case Key.Backspace:
                return KeyCode.Backspace;
            case Key.Delete:
                return KeyCode.Delete;
            case Key.Tab:
                return KeyCode.Tab;
            case Key.Enter:
                return KeyCode.Return;
            case Key.Pause:
                return KeyCode.Pause;
            case Key.Escape:
                return KeyCode.Escape;
            case Key.Space:
                return KeyCode.Space;
            case Key.NumpadPeriod:
                return KeyCode.KeypadPeriod;
            case Key.NumpadDivide:
                return KeyCode.KeypadDivide;
            case Key.NumpadMultiply:
                return KeyCode.KeypadMultiply;
            case Key.NumpadMinus:
                return KeyCode.KeypadMinus;
            case Key.NumpadPlus:
                return KeyCode.KeypadPlus;
            case Key.NumpadEnter:
                return KeyCode.KeypadEnter;
            case Key.NumpadEquals:
                return KeyCode.KeypadEquals;
            case Key.UpArrow:
                return KeyCode.UpArrow;
            case Key.DownArrow:
                return KeyCode.DownArrow;
            case Key.RightArrow:
                return KeyCode.RightArrow;
            case Key.LeftArrow:
                return KeyCode.LeftArrow;
            case Key.Insert:
                return KeyCode.Insert;
            case Key.Home:
                return KeyCode.Home;
            case Key.End:
                return KeyCode.End;
            case Key.PageUp:
                return KeyCode.PageUp;
            case Key.PageDown:
                return KeyCode.PageDown;
            case Key.Quote:
                return KeyCode.Quote;
            case Key.Comma:
                return KeyCode.Comma;
            case Key.Minus:
                return KeyCode.Minus;
            case Key.Period:
                return KeyCode.Period;
            case Key.Slash:
                return KeyCode.Slash;
            case Key.Semicolon:
                return KeyCode.Semicolon;
            case Key.Equals:
                return KeyCode.Equals;
            case Key.LeftBracket:
                return KeyCode.LeftBracket;
            case Key.Backslash:
                return KeyCode.Backslash;
            case Key.RightBracket:
                return KeyCode.RightBracket;
            case Key.Backquote:
                return KeyCode.BackQuote;
            case Key.NumLock:
                return KeyCode.Numlock;
            case Key.CapsLock:
                return KeyCode.CapsLock;
            case Key.ScrollLock:
                return KeyCode.ScrollLock;
            case Key.RightShift:
                return KeyCode.RightShift;
            case Key.LeftShift:
                return KeyCode.LeftShift;
            case Key.RightCtrl:
                return KeyCode.RightControl;
            case Key.LeftCtrl:
                return KeyCode.LeftControl;
            case Key.LeftAlt:
                return KeyCode.LeftAlt;
            case Key.PrintScreen:
                return KeyCode.Print;
            case Key.LeftApple:
            case Key.RightApple:
            case Key.AltGr:
            {
                // duplicate definitions so handle them here
                var keyCodeString = key.ToString();

                return keyCodeString switch
                {
                    nameof(Key.LeftApple) => KeyCode.LeftApple,
                    nameof(Key.LeftWindows) => KeyCode.LeftWindows,
                    nameof(Key.RightApple) => KeyCode.RightApple,
                    nameof(Key.RightWindows) => KeyCode.RightWindows,
                    nameof(Key.AltGr) => KeyCode.AltGr,
                    nameof(Key.RightAlt) => KeyCode.RightAlt,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        var alphabetRange = GetFromRange(key, Key.A, Key.Z, KeyCode.A, KeyCode.Z);
        if (alphabetRange != null)
        {
            return alphabetRange.Value;
        }

        var f1Range = GetFromRange(key, Key.F1, Key.F12, KeyCode.F1, KeyCode.F12);
        if (f1Range != null)
        {
            return f1Range.Value;
        }

        var digitRange = GetFromRange(key, Key.Digit0, Key.Digit9, KeyCode.Alpha0, KeyCode.Alpha9);

        if (digitRange != null)
        {
            // -1 and wrap back if too low
            var digitRangeValue = (int)digitRange.Value - 1;
            const int digitLowestNew = (int)KeyCode.Alpha1;
            if (digitRangeValue < digitLowestNew)
            {
                digitRangeValue = (int)KeyCode.Alpha9;
            }

            return (KeyCode)digitRangeValue;
        }

        var keypadRange = GetFromRange(key, Key.Numpad0, Key.Numpad9, KeyCode.Keypad0, KeyCode.Keypad9);

        return keypadRange;
    }

    public static KeyCode? KeyCodeParse(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;

        if (Enum.IsDefined(typeof(KeyCode), key))
        {
            return (KeyCode)Enum.Parse(typeof(KeyCode), key);
        }

        key = key.ToLower();

        // alphabet and number
        if (key.Length == 1)
        {
            var keyChar = key[0];

            // alphabet
            if (keyChar is >= 'a' and <= 'z')
            {
                return (KeyCode)(keyChar - 'a' + (int)KeyCode.A);
            }

            // number
            if (keyChar is >= '0' and <= '9')
            {
                return (KeyCode)(keyChar - '0' + (int)KeyCode.Alpha0);
            }
        }

        // numpad
        if (key.StartsWith("[") && key.EndsWith("]"))
        {
            var keyInner = key.Substring(1, key.Length - 2);
            if (int.TryParse(keyInner, NumberStyles.None, new NumberFormatInfo(), out var numpadNum))
            {
                if (numpadNum is >= 0 and <= 9)
                {
                    return (KeyCode)(numpadNum + (int)KeyCode.Keypad0);
                }
            }

            switch (keyInner)
            {
                case "+":
                    return KeyCode.KeypadPlus;
                case "-":
                    return KeyCode.KeypadMinus;
                case "*":
                    return KeyCode.KeypadMultiply;
                case "/":
                    return KeyCode.KeypadDivide;
                case ".":
                    return KeyCode.KeypadPeriod;
            }
        }

        // function key
        if (key.StartsWith("f") && int.TryParse(key.Substring(1), NumberStyles.None, new NumberFormatInfo(),
                out var numFuncKey))
        {
            if (numFuncKey is >= 1 and <= 15)
            {
                return (KeyCode)(numFuncKey - 1 + (int)KeyCode.F1);
            }
        }

        // most other key
        if (KeyStringToKeyCodeDict.TryGetValue(key, out var keyCode)) return keyCode;

        // out of range enum values
        if (!KeyStringToKeyCodeEnumsExtras.TryGetValue(key, out var keyEnumExtra)) return null;

        if (!Enum.IsDefined(typeof(KeyCode), keyEnumExtra)) return null;

        return (KeyCode)Enum.Parse(typeof(KeyCode), keyEnumExtra);
    }

    private static Key? NewKeyParse(string key)
    {
        if (Enum.IsDefined(typeof(Key), key))
        {
            return (Key)Enum.Parse(typeof(Key), key);
        }

        return null;
    }

    private static Key? NewKeyParse(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.None:
                return Key.None;
            case KeyCode.Backspace:
                return Key.Backspace;
            case KeyCode.Delete:
                return Key.Delete;
            case KeyCode.Tab:
                return Key.Tab;
            case KeyCode.Return:
                return Key.Enter;
            case KeyCode.Pause:
                return Key.Pause;
            case KeyCode.Escape:
                return Key.Escape;
            case KeyCode.Space:
                return Key.Space;
            case KeyCode.KeypadPeriod:
                return Key.NumpadPeriod;
            case KeyCode.KeypadDivide:
                return Key.NumpadDivide;
            case KeyCode.KeypadMultiply:
                return Key.NumpadMultiply;
            case KeyCode.KeypadMinus:
                return Key.NumpadMinus;
            case KeyCode.KeypadPlus:
                return Key.NumpadPlus;
            case KeyCode.KeypadEnter:
                return Key.NumpadEnter;
            case KeyCode.KeypadEquals:
                return Key.NumpadEquals;
            case KeyCode.UpArrow:
                return Key.UpArrow;
            case KeyCode.DownArrow:
                return Key.DownArrow;
            case KeyCode.RightArrow:
                return Key.RightArrow;
            case KeyCode.LeftArrow:
                return Key.LeftArrow;
            case KeyCode.Insert:
                return Key.Insert;
            case KeyCode.Home:
                return Key.Home;
            case KeyCode.End:
                return Key.End;
            case KeyCode.PageUp:
                return Key.PageUp;
            case KeyCode.PageDown:
                return Key.PageDown;
            case KeyCode.Quote:
                return Key.Quote;
            case KeyCode.Comma:
                return Key.Comma;
            case KeyCode.Minus:
                return Key.Minus;
            case KeyCode.Period:
                return Key.Period;
            case KeyCode.Slash:
                return Key.Slash;
            case KeyCode.Semicolon:
                return Key.Semicolon;
            case KeyCode.Equals:
                return Key.Equals;
            case KeyCode.LeftBracket:
                return Key.LeftBracket;
            case KeyCode.Backslash:
                return Key.Backslash;
            case KeyCode.RightBracket:
                return Key.RightBracket;
            case KeyCode.BackQuote:
                return Key.Backquote;
            case KeyCode.Numlock:
                return Key.NumLock;
            case KeyCode.CapsLock:
                return Key.CapsLock;
            case KeyCode.ScrollLock:
                return Key.ScrollLock;
            case KeyCode.RightShift:
                return Key.RightShift;
            case KeyCode.LeftShift:
                return Key.LeftShift;
            case KeyCode.RightControl:
                return Key.RightCtrl;
            case KeyCode.LeftControl:
                return Key.LeftCtrl;
            case KeyCode.RightAlt:
                return Key.RightAlt;
            case KeyCode.LeftAlt:
                return Key.LeftAlt;
            case KeyCode.LeftApple:
                return Key.LeftApple;
            case KeyCode.LeftWindows:
                return Key.LeftWindows;
            case KeyCode.RightApple:
                return Key.RightApple;
            case KeyCode.RightWindows:
                return Key.RightWindows;
            case KeyCode.AltGr:
                return Key.AltGr;
            case KeyCode.Print:
                return Key.PrintScreen;
        }

        var alphabetRange = GetFromRange(keyCode, KeyCode.A, KeyCode.Z, Key.A,
            Key.Z);
        if (alphabetRange != null)
        {
            return alphabetRange.Value;
        }

        var f1Range = GetFromRange(keyCode, KeyCode.F1, KeyCode.F12, Key.F1,
            Key.F12);
        if (f1Range != null)
        {
            return f1Range.Value;
        }

        var digitRange = GetFromRange(keyCode, KeyCode.Alpha0, KeyCode.Alpha9, Key.Digit0,
            Key.Digit9);

        if (digitRange != null)
        {
            // -1 and wrap back if too low
            var digitRangeValue = (int)digitRange.Value - 1;
            const int digitLowestNew = (int)Key.Digit1;
            if (digitRangeValue < digitLowestNew)
            {
                digitRangeValue = (int)Key.Digit9;
            }

            return (Key)digitRangeValue;
        }

        var keypadRange = GetFromRange(keyCode, KeyCode.Keypad0, KeyCode.Keypad9, Key.Numpad0,
            Key.Numpad9);

        return keypadRange;
    }

    private static Key? GetFromRange(KeyCode keyCode, KeyCode min, KeyCode max, Key minNew, Key maxNew)
    {
        var keyCodeInt = (int)keyCode;
        var minLegacyInt = (int)min;
        var maxLegacyInt = (int)max;
        var minNewInt = (int)minNew;
        var maxNewInt = (int)maxNew;

        var rangeResult = GetFromRange(keyCodeInt, minLegacyInt, maxLegacyInt, minNewInt, maxNewInt);
        return rangeResult.HasValue ? (Key)rangeResult.Value : null;
    }

    private static KeyCode? GetFromRange(Key keyCode, Key min, Key max, KeyCode minNew, KeyCode maxNew)
    {
        var keyCodeInt = (int)keyCode;
        var minLegacyInt = (int)min;
        var maxLegacyInt = (int)max;
        var minNewInt = (int)minNew;
        var maxNewInt = (int)maxNew;

        var rangeResult = GetFromRange(keyCodeInt, minLegacyInt, maxLegacyInt, minNewInt, maxNewInt);
        return rangeResult.HasValue ? (KeyCode)rangeResult.Value : null;
    }

    private static int? GetFromRange(int keyCode, int min, int max, int minNew, int maxNew)
    {
        // check if out of range key code
        if (keyCode < min || keyCode > max)
        {
            return null;
        }

        // check if out of range for new key codes
        if (keyCode - min + minNew > maxNew)
        {
            return null;
        }

        return keyCode - min + minNew;
    }
}