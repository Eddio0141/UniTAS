using System;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.UnitySafeWrappers;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers;

[Singleton]
public class CursorWrapper : ICursorWrapper
{
    // TODO test in old unity
    private readonly ILogger _logger;
    private readonly IPatchReverseInvoker _patchReverseInvoker;

    private readonly bool _newUnity;

    // new unity
    private readonly PropertyInfo _lockState;
    private readonly PropertyInfo _visible;
    private readonly Type _cursorLockMode;

    // old unity
    private readonly PropertyInfo _showCursor;
    private readonly PropertyInfo _lockCursor;

    public CursorWrapper(ILogger logger, IPatchReverseInvoker patchReverseInvoker)
    {
        _logger = logger;
        _patchReverseInvoker = patchReverseInvoker;
        var cursor = AccessTools.TypeByName("UnityEngine.Cursor");
        _lockState = AccessTools.Property(cursor, "lockState");
        _visible = AccessTools.Property(cursor, "visible");
        _newUnity = cursor != null && _lockState != null && _visible != null;
        if (_newUnity)
        {
            _cursorLockMode = _lockState!.PropertyType;
        }
        else
        {
            _showCursor = AccessTools.Property(typeof(Screen), "showCursor");
            _lockCursor = AccessTools.Property(typeof(Screen), "lockCursor");
        }
    }

    public bool Visible
    {
        get => (bool)(_newUnity ? _visible : _showCursor).GetValue(null, null);
        set => _patchReverseInvoker.Invoke(() => (_newUnity ? _visible : _showCursor).SetValue(null, value, null));
    }

    public CursorLockMode LockState
    {
        get
        {
            if (_newUnity)
            {
                var mode = _lockState.GetValue(null, null);
                return (CursorLockMode)Enum.Parse(typeof(CursorLockMode), mode.ToString());
            }

            var locked = (bool)_lockCursor.GetValue(null, null);
            return locked ? CursorLockMode.Locked : CursorLockMode.None;
        }
        set
        {
            if (_newUnity)
            {
                var mode = Enum.Parse(_cursorLockMode, value.ToString());
                _patchReverseInvoker.Invoke(() => _lockState.SetValue(null, mode, null));
            }
            else
            {
                if (value is not (CursorLockMode.Locked or CursorLockMode.None))
                {
                    _logger.LogWarning("Cursor lock mode can only be locked or none in old unity, setting to none");
                    value = CursorLockMode.None;
                }

                _patchReverseInvoker.Invoke(() => _lockCursor.SetValue(null, value == CursorLockMode.Locked, null));
            }
        }
    }
}