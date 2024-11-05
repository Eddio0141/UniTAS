using System;
using UniTAS.Patcher.Models.UnitySafeWrappers;

namespace UniTAS.Patcher.Services.GUI;

public interface IToolBar
{
    bool Show { get; }
    event Action<bool> OnShowChange;
    bool PreventCursorChange { get; }
}

public interface IActualCursorState
{
    CursorLockMode CursorLockState { set; get; }
    bool CursorVisible { set; get; }
}