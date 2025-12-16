using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Services;

namespace UniTAS.Patcher.Implementations.GUI.TerminalCommands;

// TODO: there needs to be a more robust solution
public class HookOnGameRestart(IGameRestart gameRestart) : TerminalCmd
{
    public override string Name => "hook_on_game_restart";
    public override string Description => "Hook closure on game restart. Second argument is a boolean where true will hook, and false will unhook";
    public override Delegate Callback => Execute;

    private readonly Dictionary<Closure, Services.GameRestart> _callbacks = [];

    private void Execute(Closure value, bool hook)
    {
        if (hook)
        {
            var callback = new Services.GameRestart((time, load) => value.Call(time, load));
            gameRestart.OnGameRestart += callback;
            _callbacks.Add(value, callback);
            return;
        }

        if (!_callbacks.TryGetValue(value, out var callbackFound))
        {
            return;
        }

        _callbacks.Remove(value);
        gameRestart.OnGameRestart -= callbackFound;
    }
}
