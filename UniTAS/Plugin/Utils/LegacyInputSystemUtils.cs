using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace UniTAS.Plugin.Utils;

public static class LegacyInputSystemUtils
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

    public static bool KeyStringToKeyCode(string key, out KeyCode keyCode)
    {
        // all lower case, not allowed
        keyCode = default;
        if (string.IsNullOrEmpty(key)) return false;

        // alphabet and number
        if (key.Length == 1)
        {
            var keyChar = key[0];

            // alphabet
            if (keyChar is >= 'a' and <= 'z')
            {
                keyCode = (KeyCode)(keyChar - 'a' + (int)KeyCode.A);
                return true;
            }

            // number
            if (keyChar is >= '0' and <= '9')
            {
                keyCode = (KeyCode)(keyChar - '0' + (int)KeyCode.Alpha0);
                return true;
            }
        }

        // numpad
        if (key.StartsWith("[") && key.EndsWith("]"))
        {
            var keyInner = key.Substring(1, key.Length - 2);
            if (int.TryParse(keyInner, NumberStyles.None, new NumberFormatInfo(), out var numpadNum))
            {
                if (numpadNum is < 0 or > 9) return false;
                keyCode = (KeyCode)(numpadNum + (int)KeyCode.Keypad0);
                return true;
            }

            switch (keyInner)
            {
                case "+":
                    keyCode = KeyCode.KeypadPlus;
                    break;
                case "-":
                    keyCode = KeyCode.KeypadMinus;
                    break;
                case "*":
                    keyCode = KeyCode.KeypadMultiply;
                    break;
                case "/":
                    keyCode = KeyCode.KeypadDivide;
                    break;
                case ".":
                    keyCode = KeyCode.KeypadPeriod;
                    break;
                default:
                    return false;
            }

            return true;
        }

        // function key
        if (key.StartsWith("f") && int.TryParse(key.Substring(1), NumberStyles.None, new NumberFormatInfo(),
                out var numFuncKey))
        {
            if (numFuncKey is < 1 or > 15) return false;
            keyCode = (KeyCode)(numFuncKey - 1 + (int)KeyCode.F1);
            return true;
        }

        // most other key
        if (KeyStringToKeyCodeDict.TryGetValue(key, out keyCode)) return true;

        // out of range enum values
        if (KeyStringToKeyCodeEnumsExtras.TryGetValue(key, out var keyEnumExtra))
        {
            if (!Enum.IsDefined(typeof(KeyCode), keyEnumExtra)) return false;

            keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), keyEnumExtra);
            return true;
        }

        return false;
    }
}