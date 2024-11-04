using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.Movie;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment.Input;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.Movie.Engine.Modules;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[method: MoonSharpHidden]
public class Mouse(IMouseStateEnvController mouseController)
    : EngineMethodClass
{
    public void Move(float x, float y)
    {
        var mousePos = new Vector2(x, y);
        mouseController.SetPosition(mousePos);
    }

    public void Move_rel(float x, float y)
    {
        var mousePos = new Vector2(x, y);
        mouseController.SetPositionRelative(mousePos);
    }

    public void Left(bool hold = true)
    {
        HandlePress(hold, MouseButton.Left);
    }

    public void Right(bool hold = true)
    {
        HandlePress(hold, MouseButton.Right);
    }

    public void Middle(bool hold = true)
    {
        HandlePress(hold, MouseButton.Middle);
    }

    private void HandlePress(bool hold, MouseButton button)
    {
        if (hold)
        {
            mouseController.HoldButton(button);
        }
        else
        {
            mouseController.ReleaseButton(button);
        }
    }

    public void Set_scroll(float x, float y)
    {
        mouseController.SetScroll(new(x, y));
    }
}