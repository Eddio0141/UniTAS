using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Plugin.GameEnvironment;
using UniTAS.Plugin.Interfaces.Movie;

namespace UniTAS.Plugin.Implementations.Movie.Engine.Modules;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class Mouse : EngineMethodClass
{
    private readonly VirtualEnvironment _virtualEnvironment;

    [MoonSharpHidden]
    public Mouse(VirtualEnvironment virtualEnvironment)
    {
        _virtualEnvironment = virtualEnvironment;
    }

    public void Move(float x, float y)
    {
        _virtualEnvironment.InputState.MouseState.XPos = x;
        _virtualEnvironment.InputState.MouseState.YPos = y;
    }

    public void Move_rel(float x, float y)
    {
        _virtualEnvironment.InputState.MouseState.XPos += x;
        _virtualEnvironment.InputState.MouseState.YPos += y;
    }

    public void Left(bool hold = true)
    {
        _virtualEnvironment.InputState.MouseState.LeftClick = hold;
    }

    public void Right(bool hold = true)
    {
        _virtualEnvironment.InputState.MouseState.RightClick = hold;
    }

    public void Middle(bool hold = true)
    {
        _virtualEnvironment.InputState.MouseState.MiddleClick = hold;
    }
}