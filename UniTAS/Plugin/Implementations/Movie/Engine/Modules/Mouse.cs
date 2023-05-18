using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Plugin.Interfaces.Movie;
using UniTAS.Plugin.Models.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;

namespace UniTAS.Plugin.Implementations.Movie.Engine.Modules;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class Mouse : EngineMethodClass
{
    private readonly IMouseStateEnvController _mouseController;

    [MoonSharpHidden]
    public Mouse(IMouseStateEnvController mouseController)
    {
        _mouseController = mouseController;
    }

    public void Move(float x, float y)
    {
        _mouseController.SetPosition(new(x, y));
    }

    public void Move_rel(float x, float y)
    {
        _mouseController.SetPositionRelative(new(x, y));
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
    }
}