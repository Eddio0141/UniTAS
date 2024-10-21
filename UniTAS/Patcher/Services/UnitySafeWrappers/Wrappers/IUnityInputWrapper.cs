using UnityEngine;

namespace UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;

/// <summary>
/// Gets the unity input without the TAS disrupting things
/// </summary>
public interface IUnityInputWrapper
{
    bool GetKeyDown(KeyCode keyCode);
    Vector2 MousePosition { get; }
}