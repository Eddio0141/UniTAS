using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Plugin.Interfaces.Movie;
using UniTAS.Plugin.Services.VirtualEnvironment.InnerState.Input;

namespace UniTAS.Plugin.Implementations.Movie.Engine.Modules;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class Mouse : EngineMethodClass
{
    private readonly IMouseStateEnv _mouseStateEnv;

    [MoonSharpHidden]
    public Mouse(IMouseStateEnv mouseStateEnv)
    {
        _mouseStateEnv = mouseStateEnv;
    }

    public void Move(float x, float y)
    {
        _mouseStateEnv.XPos = x;
        _mouseStateEnv.YPos = y;
    }

    public void Move_rel(float x, float y)
    {
        _mouseStateEnv.XPos += x;
        _mouseStateEnv.YPos += y;
    }

    public void Left(bool hold = true)
    {
        _mouseStateEnv.LeftClick = hold;
    }

    public void Right(bool hold = true)
    {
        _mouseStateEnv.RightClick = hold;
    }

    public void Middle(bool hold = true)
    {
        _mouseStateEnv.MiddleClick = hold;
    }
}