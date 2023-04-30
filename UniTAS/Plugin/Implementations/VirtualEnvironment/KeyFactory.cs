using System;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Models.VirtualEnvironment;
using UniTAS.Plugin.Services.InputSystemOverride;
using UniTAS.Plugin.Services.Logging;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UnityEngine;

namespace UniTAS.Plugin.Implementations.VirtualEnvironment;

[Singleton]
public class KeyFactory : IKeyFactory
{
    private readonly IInputSystemExists _inputSystemExists;
    private readonly ILogger _logger;

    public KeyFactory(IInputSystemExists inputSystemExists, ILogger logger)
    {
        _inputSystemExists = inputSystemExists;
        _logger = logger;
    }

    public Key CreateKey(string key)
    {
        var keyCode = ParseKeyCode(key);
        return keyCode.HasValue ? CreateKey(keyCode.Value) : new(key, null);
    }

    public Key CreateKey(KeyCode key)
    {
        return new(key, _inputSystemExists.HasInputSystem ? NewKeyFromKeyCode(key) : null);
    }

    private static KeyCode? ParseKeyCode(string key)
    {
        var keyClean = key.Trim().ToLower();
        if (Enum.IsDefined(typeof(KeyCode), keyClean))
        {
            return (KeyCode)Enum.Parse(typeof(KeyCode), keyClean, true);
        }

        return null;
    }

    private UnityEngine.InputSystem.Key? NewKeyFromKeyCode(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.None:
                return UnityEngine.InputSystem.Key.None;
            case KeyCode.Backspace:
                return UnityEngine.InputSystem.Key.Backspace;
            case KeyCode.Delete:
                return UnityEngine.InputSystem.Key.Delete;
            case KeyCode.Tab:
                return UnityEngine.InputSystem.Key.Tab;
            case KeyCode.Return:
                return UnityEngine.InputSystem.Key.Enter;
            case KeyCode.Pause:
                return UnityEngine.InputSystem.Key.Pause;
            case KeyCode.Escape:
                return UnityEngine.InputSystem.Key.Escape;
            case KeyCode.Space:
                return UnityEngine.InputSystem.Key.Space;
            case KeyCode.KeypadPeriod:
                return UnityEngine.InputSystem.Key.NumpadPeriod;
            case KeyCode.KeypadDivide:
                return UnityEngine.InputSystem.Key.NumpadDivide;
            case KeyCode.KeypadMultiply:
                return UnityEngine.InputSystem.Key.NumpadMultiply;
            case KeyCode.KeypadMinus:
                return UnityEngine.InputSystem.Key.NumpadMinus;
            case KeyCode.KeypadPlus:
                return UnityEngine.InputSystem.Key.NumpadPlus;
            case KeyCode.KeypadEnter:
                return UnityEngine.InputSystem.Key.NumpadEnter;
            case KeyCode.KeypadEquals:
                return UnityEngine.InputSystem.Key.NumpadEquals;
            case KeyCode.UpArrow:
                return UnityEngine.InputSystem.Key.UpArrow;
            case KeyCode.DownArrow:
                return UnityEngine.InputSystem.Key.DownArrow;
            case KeyCode.RightArrow:
                return UnityEngine.InputSystem.Key.RightArrow;
            case KeyCode.LeftArrow:
                return UnityEngine.InputSystem.Key.LeftArrow;
            case KeyCode.Insert:
                return UnityEngine.InputSystem.Key.Insert;
            case KeyCode.Home:
                return UnityEngine.InputSystem.Key.Home;
            case KeyCode.End:
                return UnityEngine.InputSystem.Key.End;
            case KeyCode.PageUp:
                return UnityEngine.InputSystem.Key.PageUp;
            case KeyCode.PageDown:
                return UnityEngine.InputSystem.Key.PageDown;
            case KeyCode.Quote:
                return UnityEngine.InputSystem.Key.Quote;
            case KeyCode.Comma:
                return UnityEngine.InputSystem.Key.Comma;
            case KeyCode.Minus:
                return UnityEngine.InputSystem.Key.Minus;
            case KeyCode.Period:
                return UnityEngine.InputSystem.Key.Period;
            case KeyCode.Slash:
                return UnityEngine.InputSystem.Key.Slash;
            case KeyCode.Semicolon:
                return UnityEngine.InputSystem.Key.Semicolon;
            case KeyCode.Equals:
                return UnityEngine.InputSystem.Key.Equals;
            case KeyCode.LeftBracket:
                return UnityEngine.InputSystem.Key.LeftBracket;
            case KeyCode.Backslash:
                return UnityEngine.InputSystem.Key.Backslash;
            case KeyCode.RightBracket:
                return UnityEngine.InputSystem.Key.RightBracket;
            case KeyCode.BackQuote:
                return UnityEngine.InputSystem.Key.Backquote;
            case KeyCode.Numlock:
                return UnityEngine.InputSystem.Key.NumLock;
            case KeyCode.CapsLock:
                return UnityEngine.InputSystem.Key.CapsLock;
            case KeyCode.ScrollLock:
                return UnityEngine.InputSystem.Key.ScrollLock;
            case KeyCode.RightShift:
                return UnityEngine.InputSystem.Key.RightShift;
            case KeyCode.LeftShift:
                return UnityEngine.InputSystem.Key.LeftShift;
            case KeyCode.RightControl:
                return UnityEngine.InputSystem.Key.RightCtrl;
            case KeyCode.LeftControl:
                return UnityEngine.InputSystem.Key.LeftCtrl;
            case KeyCode.RightAlt:
                return UnityEngine.InputSystem.Key.RightAlt;
            case KeyCode.LeftAlt:
                return UnityEngine.InputSystem.Key.LeftAlt;
            case KeyCode.LeftApple:
                return UnityEngine.InputSystem.Key.LeftApple;
            case KeyCode.LeftWindows:
                return UnityEngine.InputSystem.Key.LeftWindows;
            case KeyCode.RightApple:
                return UnityEngine.InputSystem.Key.RightApple;
            case KeyCode.RightWindows:
                return UnityEngine.InputSystem.Key.RightWindows;
            case KeyCode.AltGr:
                return UnityEngine.InputSystem.Key.AltGr;
            case KeyCode.Print:
                return UnityEngine.InputSystem.Key.PrintScreen;
        }

        var alphabetRange = GetFromRange(keyCode, KeyCode.A, KeyCode.Z, UnityEngine.InputSystem.Key.A,
            UnityEngine.InputSystem.Key.Z);
        if (alphabetRange != null)
        {
            return alphabetRange.Value;
        }

        var f1Range = GetFromRange(keyCode, KeyCode.F1, KeyCode.F12, UnityEngine.InputSystem.Key.F1,
            UnityEngine.InputSystem.Key.F12);
        if (f1Range != null)
        {
            return f1Range.Value;
        }

        var digitRange = GetFromRange(keyCode, KeyCode.Alpha0, KeyCode.Alpha9, UnityEngine.InputSystem.Key.Digit0,
            UnityEngine.InputSystem.Key.Digit9);

        if (digitRange != null)
        {
            // -1 and wrap back if too low
            var digitRangeValue = (int)digitRange.Value - 1;
            const int digitLowestNew = (int)UnityEngine.InputSystem.Key.Digit1;
            if (digitRangeValue < digitLowestNew)
            {
                digitRangeValue = (int)UnityEngine.InputSystem.Key.Digit9;
            }

            return (UnityEngine.InputSystem.Key)digitRangeValue;
        }

        var keypadRange = GetFromRange(keyCode, KeyCode.Keypad0, KeyCode.Keypad9, UnityEngine.InputSystem.Key.Numpad0,
            UnityEngine.InputSystem.Key.Numpad9);

        if (keypadRange != null)
        {
            return keypadRange.Value;
        }

        _logger.LogWarning($"Key code {keyCode} not found in legacy to new key code mapping");

        return null;
    }

    private static UnityEngine.InputSystem.Key? GetFromRange(KeyCode keyCode, KeyCode min, KeyCode max,
        UnityEngine.InputSystem.Key minNew, UnityEngine.InputSystem.Key maxNew)
    {
        var keyCodeInt = (int)keyCode;
        var minLegacyInt = (int)min;
        var maxLegacyInt = (int)max;
        var minNewInt = (int)minNew;
        var maxNewInt = (int)maxNew;

        // check if out of range key code
        if (keyCodeInt < minLegacyInt || keyCodeInt > maxLegacyInt)
        {
            return null;
        }

        // check if out of range for new key codes
        if (keyCodeInt - minLegacyInt + minNewInt > maxNewInt)
        {
            return null;
        }

        return (UnityEngine.InputSystem.Key)(keyCodeInt - minLegacyInt + minNewInt);
    }
}