using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Services.UnityEvents;

namespace UniTAS.Patcher.Implementations.GUI.TerminalCommands;

public class EventCoroutine(IUpdateEvents updateEvents) : TerminalCmd
{
    public override string Name => "event_coroutine";

    public override string Description =>
        "starts a coroutine runner with ability to yield on events. Arg0 (function): coroutine function. the following is the list of allowed yield string values: [update_unconditional]";

    public override Delegate Callback => Execute;

    private int _coroutineCount;

    private void Execute(Script script, Closure callback)
    {
        var coroutine = script.CreateCoroutine(callback).Coroutine;

        if (_coroutineCount == 0)
        {
            updateEvents.OnUpdateUnconditional += UpdateUnconditional;
        }

        _coroutineCount++;

        HandleCoroutine(coroutine);
    }

    private void HandleCoroutine(MoonSharp.Interpreter.Coroutine coroutine)
    {
        if (coroutine.State == CoroutineState.Dead)
        {
            return;
        }

        DynValue resumeValue;
        try
        {
            resumeValue = coroutine.Resume();
        }
        catch (Exception)
        {
            CoroutineEnd();
            throw;
        }

        var yieldRaw = resumeValue.CastToString().Trim();
        if (!Enum.IsDefined(typeof(Event), yieldRaw))
        {
            CoroutineEnd();
            throw new Exception($"Event coroutine '{yieldRaw}' is not defined.");
        }

        var yield = (Event)Enum.Parse(typeof(Event), yieldRaw);
        switch (yield)
        {
            case Event.UpdateUnconditional:
                _updateUnconditionalCallbacks.Enqueue(coroutine);
                break;
            default:
                CoroutineEnd();
                throw new ArgumentOutOfRangeException();
        }
    }

    private void CoroutineEnd()
    {
        _coroutineCount--;
        if (_coroutineCount > 0) return;

        updateEvents.OnUpdateUnconditional -= UpdateUnconditional;
    }

    private readonly Queue<MoonSharp.Interpreter.Coroutine> _updateUnconditionalCallbacks = new();

    private void UpdateUnconditional()
    {
        var count = _updateUnconditionalCallbacks.Count;
        for (var i = 0; i < count; i++)
        {
            HandleCoroutine(_updateUnconditionalCallbacks.Dequeue());
        }
    }

    private enum Event
    {
        UpdateUnconditional
    }
}