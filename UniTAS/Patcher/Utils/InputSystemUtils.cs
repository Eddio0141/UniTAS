using System;
using System.Collections.Generic;
using System.Globalization;
using UniTAS.Patcher.Extensions;
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
            newKey ??= NewKeyParse(keyCode.Value);
            return;
        }

        if (newKey.HasValue)
        {
            keyCode ??= KeyCodeParse(newKey.Value);
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
            // easier to do here
            case Key.Digit0:
                return KeyCode.Alpha0;
            case Key.Digit1:
                return KeyCode.Alpha1;
            case Key.Digit2:
                return KeyCode.Alpha2;
            case Key.Digit3:
                return KeyCode.Alpha3;
            case Key.Digit4:
                return KeyCode.Alpha4;
            case Key.Digit5:
                return KeyCode.Alpha5;
            case Key.Digit6:
                return KeyCode.Alpha6;
            case Key.Digit7:
                return KeyCode.Alpha7;
            case Key.Digit8:
                return KeyCode.Alpha8;
            case Key.Digit9:
                return KeyCode.Alpha9;
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
            case Key.A:
                return KeyCode.A;
            case Key.B:
                return KeyCode.B;
            case Key.C:
                return KeyCode.C;
            case Key.D:
                return KeyCode.D;
            case Key.E:
                return KeyCode.E;
            case Key.F:
                return KeyCode.F;
            case Key.G:
                return KeyCode.G;
            case Key.H:
                return KeyCode.H;
            case Key.I:
                return KeyCode.I;
            case Key.J:
                return KeyCode.J;
            case Key.K:
                return KeyCode.K;
            case Key.L:
                return KeyCode.L;
            case Key.M:
                return KeyCode.M;
            case Key.N:
                return KeyCode.N;
            case Key.O:
                return KeyCode.O;
            case Key.P:
                return KeyCode.P;
            case Key.Q:
                return KeyCode.Q;
            case Key.R:
                return KeyCode.R;
            case Key.S:
                return KeyCode.S;
            case Key.T:
                return KeyCode.T;
            case Key.U:
                return KeyCode.U;
            case Key.V:
                return KeyCode.V;
            case Key.W:
                return KeyCode.W;
            case Key.X:
                return KeyCode.X;
            case Key.Y:
                return KeyCode.Y;
            case Key.Z:
                return KeyCode.Z;
            case Key.F1:
                return KeyCode.F1;
            case Key.F2:
                return KeyCode.F2;
            case Key.F3:
                return KeyCode.F3;
            case Key.F4:
                return KeyCode.F4;
            case Key.F5:
                return KeyCode.F5;
            case Key.F6:
                return KeyCode.F6;
            case Key.F7:
                return KeyCode.F7;
            case Key.F8:
                return KeyCode.F8;
            case Key.F9:
                return KeyCode.F9;
            case Key.F10:
                return KeyCode.F10;
            case Key.F11:
                return KeyCode.F11;
            case Key.F12:
                return KeyCode.F12;
            case Key.Numpad0:
                return KeyCode.Keypad0;
            case Key.Numpad1:
                return KeyCode.Keypad1;
            case Key.Numpad2:
                return KeyCode.Keypad2;
            case Key.Numpad3:
                return KeyCode.Keypad3;
            case Key.Numpad4:
                return KeyCode.Keypad4;
            case Key.Numpad5:
                return KeyCode.Keypad5;
            case Key.Numpad6:
                return KeyCode.Keypad6;
            case Key.Numpad7:
                return KeyCode.Keypad7;
            case Key.Numpad8:
                return KeyCode.Keypad8;
            case Key.Numpad9:
                return KeyCode.Keypad9;
            case Key.ContextMenu:
            case Key.OEM1:
            case Key.OEM2:
            case Key.OEM3:
            case Key.OEM4:
            case Key.OEM5:
            case Key.IMESelected:
            default:
                return null;
        }
    }

    public static KeyCode? KeyCodeParse(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;

        if (EnumUtils.TryParse(key, out KeyCode parsedKeyCode, true)) return parsedKeyCode;

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

    public static string KeyCodeToStringVariant(KeyCode keyCode)
    {
        // convert literal name to lowercase and return
        if (keyCode is >= KeyCode.A and <= KeyCode.Z or >= KeyCode.F1 and <= KeyCode.F15 or KeyCode.Backspace
            or KeyCode.Tab or KeyCode.Return or KeyCode.Escape or KeyCode.Space or KeyCode.Delete or KeyCode.Insert
            or KeyCode.Home or KeyCode.End or KeyCode.Numlock or KeyCode.Help)
        {
            return keyCode.ToString().ToLowerInvariant();
        }

        // number
        if (keyCode is >= KeyCode.Alpha0 and <= KeyCode.Alpha9)
        {
            var num = KeyCode.Alpha9 - keyCode;
            return num.ToString();
        }

        // keypad number
        if (keyCode is >= KeyCode.Keypad0 and <= KeyCode.Keypad9)
        {
            var num = KeyCode.Keypad9 - keyCode;
            return $"[{num}]";
        }

        // specific
        switch (keyCode)
        {
            case KeyCode.KeypadPeriod:
                return "[.]";
            case KeyCode.KeypadPlus:
                return "[+]";
            case KeyCode.KeypadMinus:
                return "[-]";
            case KeyCode.KeypadMultiply:
                return "[*]";
            case KeyCode.KeypadDivide:
                return "[/]";
            case KeyCode.KeypadEquals:
                return "[equals]";
            case KeyCode.UpArrow:
                return "up";
            case KeyCode.DownArrow:
                return "down";
            case KeyCode.LeftArrow:
                return "left";
            case KeyCode.RightArrow:
                return "right";
            case KeyCode.Minus:
                return "-";
            case KeyCode.Equals:
                return "=";
            case KeyCode.Exclaim:
                return "!";
            case KeyCode.At:
                return "@";
            case KeyCode.Hash:
                return "#";
            case KeyCode.Dollar:
                return "$";
            case KeyCode.Caret:
                return "^";
            case KeyCode.Ampersand:
                return "&";
            case KeyCode.Asterisk:
                return "*";
            case KeyCode.LeftParen:
                return "(";
            case KeyCode.RightParen:
                return ")";
            case KeyCode.Underscore:
                return "_";
            case KeyCode.Plus:
                return "+";
            case KeyCode.LeftBracket:
                return "[";
            case KeyCode.RightBracket:
                return "]";
            case KeyCode.BackQuote:
                return "`";
            case KeyCode.Semicolon:
                return ";";
            case KeyCode.Quote:
                return "'";
            case KeyCode.Backslash:
                return "\\";
            case KeyCode.Colon:
                return ":";
            case KeyCode.DoubleQuote:
                return "\"";
            case KeyCode.Comma:
                return ",";
            case KeyCode.Period:
                return ".";
            case KeyCode.Slash:
                return "/";
            case KeyCode.Less:
                return "<";
            case KeyCode.Greater:
                return ">";
            case KeyCode.Question:
                return "?";
            case KeyCode.RightApple:
                return "right cmd";
            case KeyCode.LeftApple:
                return "left cmd";
            case KeyCode.RightWindows:
                return "right super";
            case KeyCode.LeftWindows:
                return "left super";
            case KeyCode.Print:
                return "print screen";
        }

        // CamelCase split by each capital letter, and number, which is then made into lower case
        var keyCodeString = keyCode.ToString();
        var capLetterIndexes = keyCodeString.AllIndexesOfAny(CharUtils.AlphabetCapital, 1);
        for (var i = 0; i < capLetterIndexes.Length; i++)
        {
            var chIndex = capLetterIndexes[i] + i;
            var c = keyCodeString[chIndex];
            keyCodeString = keyCodeString.Remove(chIndex, 1).Insert(chIndex, $" {char.ToLowerInvariant(c)}");
        }

        var numIndexes = keyCodeString.AllIndexesOfAny(CharUtils.Numbers, 1);
        for (var i = 0; i < numIndexes.Length; i++)
        {
            var chIndex = numIndexes[i] + i;
            var c = keyCodeString[chIndex];
            keyCodeString = keyCodeString.Remove(chIndex, 1).Insert(chIndex, $" {c}");
        }

        // first char to lowercase
        if (keyCodeString.Length > 0)
        {
            var c = keyCodeString[0];
            keyCodeString = keyCodeString.Remove(0, 1).Insert(0, $"{char.ToLowerInvariant(c)}");
        }

        return keyCodeString;
    }

    private static Key? NewKeyParse(string key)
    {
        if (EnumUtils.TryParse(key, out Key result, true))
        {
            return result;
        }

        return null;
    }

    public static Key? NewKeyParse(KeyCode keyCode)
    {
        return keyCode switch
        {
            KeyCode.None => Key.None,
            KeyCode.Backspace => Key.Backspace,
            KeyCode.Delete => Key.Delete,
            KeyCode.Tab => Key.Tab,
            KeyCode.Return => Key.Enter,
            KeyCode.Pause => Key.Pause,
            KeyCode.Escape => Key.Escape,
            KeyCode.Space => Key.Space,
            KeyCode.KeypadPeriod => Key.NumpadPeriod,
            KeyCode.KeypadDivide => Key.NumpadDivide,
            KeyCode.KeypadMultiply => Key.NumpadMultiply,
            KeyCode.KeypadMinus => Key.NumpadMinus,
            KeyCode.KeypadPlus => Key.NumpadPlus,
            KeyCode.KeypadEnter => Key.NumpadEnter,
            KeyCode.KeypadEquals => Key.NumpadEquals,
            KeyCode.UpArrow => Key.UpArrow,
            KeyCode.DownArrow => Key.DownArrow,
            KeyCode.RightArrow => Key.RightArrow,
            KeyCode.LeftArrow => Key.LeftArrow,
            KeyCode.Insert => Key.Insert,
            KeyCode.Home => Key.Home,
            KeyCode.End => Key.End,
            KeyCode.PageUp => Key.PageUp,
            KeyCode.PageDown => Key.PageDown,
            KeyCode.Quote => Key.Quote,
            KeyCode.Comma => Key.Comma,
            KeyCode.Minus => Key.Minus,
            KeyCode.Period => Key.Period,
            KeyCode.Slash => Key.Slash,
            KeyCode.Semicolon => Key.Semicolon,
            KeyCode.Equals => Key.Equals,
            KeyCode.LeftBracket => Key.LeftBracket,
            KeyCode.Backslash => Key.Backslash,
            KeyCode.RightBracket => Key.RightBracket,
            KeyCode.BackQuote => Key.Backquote,
            KeyCode.Numlock => Key.NumLock,
            KeyCode.CapsLock => Key.CapsLock,
            KeyCode.ScrollLock => Key.ScrollLock,
            KeyCode.RightShift => Key.RightShift,
            KeyCode.LeftShift => Key.LeftShift,
            KeyCode.RightControl => Key.RightCtrl,
            KeyCode.LeftControl => Key.LeftCtrl,
            KeyCode.RightAlt => Key.RightAlt,
            KeyCode.LeftAlt => Key.LeftAlt,
            KeyCode.LeftApple => Key.LeftApple,
            KeyCode.LeftWindows => Key.LeftWindows,
            KeyCode.RightApple => Key.RightApple,
            KeyCode.RightWindows => Key.RightWindows,
            KeyCode.AltGr => Key.AltGr,
            KeyCode.Print => Key.PrintScreen,
            // easier to do here
            KeyCode.Alpha0 => Key.Digit0,
            KeyCode.Alpha1 => Key.Digit1,
            KeyCode.Alpha2 => Key.Digit2,
            KeyCode.Alpha3 => Key.Digit3,
            KeyCode.Alpha4 => Key.Digit4,
            KeyCode.Alpha5 => Key.Digit5,
            KeyCode.Alpha6 => Key.Digit6,
            KeyCode.Alpha7 => Key.Digit7,
            KeyCode.Alpha8 => Key.Digit8,
            KeyCode.Alpha9 => Key.Digit9,
            KeyCode.A => Key.A,
            KeyCode.B => Key.B,
            KeyCode.C => Key.C,
            KeyCode.D => Key.D,
            KeyCode.E => Key.E,
            KeyCode.F => Key.F,
            KeyCode.G => Key.G,
            KeyCode.H => Key.H,
            KeyCode.I => Key.I,
            KeyCode.J => Key.J,
            KeyCode.K => Key.K,
            KeyCode.L => Key.L,
            KeyCode.M => Key.M,
            KeyCode.N => Key.N,
            KeyCode.O => Key.O,
            KeyCode.P => Key.P,
            KeyCode.Q => Key.Q,
            KeyCode.R => Key.R,
            KeyCode.S => Key.S,
            KeyCode.T => Key.T,
            KeyCode.U => Key.U,
            KeyCode.V => Key.V,
            KeyCode.W => Key.W,
            KeyCode.X => Key.X,
            KeyCode.Y => Key.Y,
            KeyCode.Z => Key.Z,
            KeyCode.F1 => Key.F1,
            KeyCode.F2 => Key.F2,
            KeyCode.F3 => Key.F3,
            KeyCode.F4 => Key.F4,
            KeyCode.F5 => Key.F5,
            KeyCode.F6 => Key.F6,
            KeyCode.F7 => Key.F7,
            KeyCode.F8 => Key.F8,
            KeyCode.F9 => Key.F9,
            KeyCode.F10 => Key.F10,
            KeyCode.F11 => Key.F11,
            KeyCode.F12 => Key.F12,
            KeyCode.Keypad0 => Key.Numpad0,
            KeyCode.Keypad1 => Key.Numpad1,
            KeyCode.Keypad2 => Key.Numpad2,
            KeyCode.Keypad3 => Key.Numpad3,
            KeyCode.Keypad4 => Key.Numpad4,
            KeyCode.Keypad5 => Key.Numpad5,
            KeyCode.Keypad6 => Key.Numpad6,
            KeyCode.Keypad7 => Key.Numpad7,
            KeyCode.Keypad8 => Key.Numpad8,
            KeyCode.Keypad9 => Key.Numpad9,
            _ => null
        };
    }
}