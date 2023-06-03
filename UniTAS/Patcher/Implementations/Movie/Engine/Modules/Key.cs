using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.Movie;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment.Input;

namespace UniTAS.Patcher.Implementations.Movie.Engine.Modules;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class Key : EngineMethodClass
{
    private readonly IKeyboardStateEnvController _kbController;
    private readonly IKeyFactory _keyFactory;

    [MoonSharpHidden]
    public Key(IKeyboardStateEnvController kbController, IKeyFactory keyFactory)
    {
        _kbController = kbController;
        _keyFactory = keyFactory;
    }

    public void Hold(string key)
    {
        _kbController.Hold(_keyFactory.CreateKey(key));
    }

    public void Release(string key)
    {
        _kbController.Release(_keyFactory.CreateKey(key));
    }

    public void Clear()
    {
        _kbController.Clear();
    }
}