using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.Movie;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment.Input;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.Movie.Engine.Modules;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class Mouse : EngineMethodClass
{
    private readonly IMouseStateEnvController _mouseController;
    private readonly IAxisStateEnvLegacySystem _axisStateEnvLegacySystem;

    [MoonSharpHidden]
    public Mouse(IMouseStateEnvController mouseController, IAxisStateEnvLegacySystem axisStateEnvLegacySystem)
    {
        _mouseController = mouseController;
        _axisStateEnvLegacySystem = axisStateEnvLegacySystem;
    }

    public void Move(float x, float y)
    {
        var mousePos = new Vector2(x, y);
        _mouseController.SetPosition(mousePos);
        _axisStateEnvLegacySystem.MouseMove(mousePos);
    }

    public void Move_rel(float x, float y)
    {
        var mousePos = new Vector2(x, y);
        _mouseController.SetPositionRelative(mousePos);
        _axisStateEnvLegacySystem.MouseMoveRelative(mousePos);
    }

    public void Left(bool hold = true)
    {
        if (hold)
        {
            _mouseController.HoldButton(MouseButton.Left);
        }
        else
        {
            _mouseController.ReleaseButton(MouseButton.Left);
        }
    }

    public void Right(bool hold = true)
    {
        if (hold)
        {
            _mouseController.HoldButton(MouseButton.Right);
        }
        else
        {
            _mouseController.ReleaseButton(MouseButton.Right);
        }
    }

    public void Middle(bool hold = true)
    {
        if (hold)
        {
            _mouseController.HoldButton(MouseButton.Middle);
        }
        else
        {
            _mouseController.ReleaseButton(MouseButton.Middle);
        }
    }

    public void Set_scroll(float x, float y)
    {
        _mouseController.SetScroll(new(x, y));
        _axisStateEnvLegacySystem.MouseScroll(y);
    }
}