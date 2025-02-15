using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Services.UnityEvents;

namespace UniTAS.Patcher.Implementations.GUI.TerminalCommands;

public class EventCoroutine : TerminalCmd
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
            _updateEvents.OnUpdateUnconditional += UpdateUnconditional;
            _updateEvents.OnUpdateActual += UpdateActual;
            _updateEvents.OnFixedUpdateActual += FixedUpdateActual;
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

        var yield = (int)(Event)Enum.Parse(typeof(Event), yieldRaw);
        if (yield >= Enum.GetValues(typeof(Event)).Length)
        {
            CoroutineEnd();
            throw new Exception($"Event coroutine '{yieldRaw}' is out of range.");
        }

        _callbacks[yield].Enqueue(coroutine);
    }

    private void CoroutineEnd()
    {
        _coroutineCount--;
        if (_coroutineCount > 0) return;

        _updateEvents.OnUpdateUnconditional -= UpdateUnconditional;
        _updateEvents.OnUpdateActual -= UpdateActual;
        _updateEvents.OnFixedUpdateActual -= FixedUpdateActual;
    }

    private readonly Queue<MoonSharp.Interpreter.Coroutine>[] _callbacks;
    private readonly IUpdateEvents _updateEvents;

    public EventCoroutine(IUpdateEvents updateEvents)
    {
        _updateEvents = updateEvents;
        var events = Enum.GetValues(typeof(Event));
        _callbacks = new Queue<MoonSharp.Interpreter.Coroutine>[events.Length];
        for (var i = 0; i < events.Length; i++)
        {
            _callbacks[i] = new();
        }
    }

    private void UpdateUnconditional() => HandleCoroutine(Event.UpdateUnconditional);
    private void UpdateActual() => HandleCoroutine(Event.UpdateActual);
    private void FixedUpdateActual() => HandleCoroutine(Event.FixedUpdateActual);

    private void HandleCoroutine(Event @event)
    {
        var callbacks = _callbacks[(int)@event];
        while (callbacks.Count > 0)
            HandleCoroutine(callbacks.Dequeue());
    }

    private enum Event
    {
        UpdateActual,
        UpdateUnconditional,
        FixedUpdateActual,
    }
}