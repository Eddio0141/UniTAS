using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Plugin.Interfaces.Movie;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;

namespace UniTAS.Plugin.Implementations.Movie.Engine.Modules;

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