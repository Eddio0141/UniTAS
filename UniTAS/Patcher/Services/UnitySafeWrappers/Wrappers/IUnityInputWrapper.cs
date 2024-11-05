using UnityEngine;

namespace UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;

/// <summary>
/// Gets the unity input without the TAS disrupting things
/// </summary>
public interface IUnityInputWrapper
{
    bool GetKeyDown(KeyCode keyCode, bool reverseInvoke = true);
    Vector2 GetMousePosition(bool reverseInvoke = true);
    bool GetAnyKeyDown(bool reverseInvoke = true);
    bool GetMouseButtonDown(int button, bool reverseInvoke = true);
    bool GetMouseButton(int button, bool reverseInvoke = true);
}