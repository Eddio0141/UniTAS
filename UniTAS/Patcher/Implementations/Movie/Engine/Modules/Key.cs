using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.Movie;
using UniTAS.Patcher.Services.VirtualEnvironment.Input;

namespace UniTAS.Patcher.Implementations.Movie.Engine.Modules;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class Key : EngineMethodClass
{
    private readonly IKeyboardStateEnvController _kbController;

    [MoonSharpHidden]
    public Key(IKeyboardStateEnvController kbController)
    {
        _kbController = kbController;
    }

    public void Hold(string key)
    {
        _kbController.Hold(key);
    }

    public void Release(string key)
    {
        _kbController.Release(key);
    }

    public void Clear()
    {
        _kbController.Clear();
    }
}