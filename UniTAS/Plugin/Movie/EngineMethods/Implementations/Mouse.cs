using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.GameEnvironment;

namespace UniTAS.Plugin.Movie.EngineMethods.Implementations;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class Mouse : EngineMethodClass
{
    private readonly VirtualEnvironment _virtualEnvironment;

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