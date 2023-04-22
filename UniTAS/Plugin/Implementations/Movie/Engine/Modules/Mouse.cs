using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Plugin.Interfaces.Movie;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;
using UnityEngine;

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
        _mouseStateEnv.Position = new(x, y);
    }

    public void Move_rel(float x, float y)
    {
        _mouseStateEnv.Position += new Vector2(x, y);
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

    public void Set_scroll(float x, float y)
    {
        _mouseStateEnv.Scroll = new(x, y);
    }
}