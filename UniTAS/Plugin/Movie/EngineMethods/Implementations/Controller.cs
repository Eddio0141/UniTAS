using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.GameEnvironment;

namespace UniTAS.Plugin.Movie.EngineMethods.Implementations;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class Controller : EngineMethodClass
{
    private readonly VirtualEnvironment _virtualEnvironment;

    public Controller(VirtualEnvironment virtualEnvironment)
    {
        _virtualEnvironment = virtualEnvironment;
    }

    public override string ClassName => "controller";

    public void Axis(string axis, float value)
    {
        _virtualEnvironment.InputState.AxisState.Values[axis] = value;
    }

    public void Hold(string button)
    {
        _virtualEnvironment.InputState.ButtonState.Hold(button);
    }

    public void Release(string button)
    {
        _virtualEnvironment.InputState.ButtonState.Release(button);
    }

    public void Clear_buttons()
    {
        _virtualEnvironment.InputState.ButtonState.Clear();
    }
}