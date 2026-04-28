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
public class Mouse(IMouseState mouse)
    : EngineMethodClass
{
    public void Move(float x, float y)
    {
        mouse.Position = new(x, y);
    }

    public void Move_rel(float x, float y)
    {
        mouse.Position += new Vector2(x, y);
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
            mouse.HoldButton(button);
        }
        else
        {
            mouse.ReleaseButton(button);
        }
    }

    public void Set_scroll(float x, float y)
    {
        mouse.Scroll = new(x, y);
    }
}
