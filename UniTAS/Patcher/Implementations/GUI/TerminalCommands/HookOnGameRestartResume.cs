using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Services;

namespace UniTAS.Patcher.Implementations.GUI.TerminalCommands;

// TODO: there needs to be a more robust solution
public class HookOnGameRestartResume(IGameRestart gameRestart) : TerminalCmd
{
    public override string Name => "hook_on_game_restart_resume";
    public override string Description => "cast types";
    public override Delegate Callback => Execute;

    private readonly Dictionary<Closure, GameRestartResume> _callbacks = [];

    private void Execute(Script script, Closure value, bool hook)
    {
        if (hook)
        {
            var callback = new GameRestartResume((time, load) => value.Call(time, load));
            gameRestart.OnGameRestartResume += callback;
            _callbacks.Add(value, callback);
            return;
        }

        if (!_callbacks.TryGetValue(value, out var callbackFound))
        {
            return;
        }

        _callbacks.Remove(value);
        gameRestart.OnGameRestartResume -= callbackFound;
    }
}