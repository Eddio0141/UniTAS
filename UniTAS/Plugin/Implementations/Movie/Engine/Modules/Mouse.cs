using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Plugin.Interfaces.Movie;
using UniTAS.Plugin.Models.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UnityEngine;

namespace UniTAS.Plugin.Implementations.Movie.Engine.Modules;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class Mouse : EngineMethodClass
{
    private readonly IMouseStateEnvLegacySystem _mouseStateEnvLegacySystem;

    [MoonSharpHidden]
    public Mouse(IMouseStateEnvLegacySystem mouseStateEnvLegacySystem)
    {
        _mouseStateEnvLegacySystem = mouseStateEnvLegacySystem;
    }

    public void Move(float x, float y)
    {
        _mouseStateEnvLegacySystem.Position = new(x, y);
    }

    public void Move_rel(float x, float y)
    {
        _mouseStateEnvLegacySystem.Position += new Vector2(x, y);
    }

    public void Left(bool hold = true)
    {
        _mouseStateEnvLegacySystem.HoldButton(MouseButton.Left);
    }

    public void Right(bool hold = true)
    {
        _mouseStateEnvLegacySystem.HoldButton(MouseButton.Right);
    }

    public void Middle(bool hold = true)
    {
        _mouseStateEnvLegacySystem.HoldButton(MouseButton.Middle);
    }

    public void Set_scroll(float x, float y)
    {
        _mouseStateEnvLegacySystem.Scroll = new(x, y);
    }
}