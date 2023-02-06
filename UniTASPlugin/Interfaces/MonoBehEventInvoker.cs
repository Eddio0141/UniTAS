using System.Diagnostics;
using UniTASPlugin.Interfaces.StartEvent;
using UniTASPlugin.Interfaces.Update;

namespace UniTASPlugin.Interfaces;

// ReSharper disable once ClassNeverInstantiated.Global
public class MonoBehEventInvoker : IMonoBehEventInvoker
{
    private bool _updated;
    private bool _calledPreUpdate;

    private readonly IOnAwake[] _onAwakes;
    private readonly IOnStart[] _onStarts;
    private readonly IOnEnable[] _onEnables;
    private readonly IOnPreUpdates[] _onPreUpdates;
    private readonly IOnUpdate[] _onUpdates;
    private readonly IOnFixedUpdate[] _onFixedUpdates;

    public MonoBehEventInvoker(IOnAwake[] onAwakes, IOnStart[] onStarts, IOnEnable[] onEnables,
        IOnPreUpdates[] onPreUpdates, IOnUpdate[] onUpdates, IOnFixedUpdate[] onFixedUpdates)
    {
        _onAwakes = onAwakes;
        _onStarts = onStarts;
        _onEnables = onEnables;
        _onPreUpdates = onPreUpdates;
        _onUpdates = onUpdates;
        _onFixedUpdates = onFixedUpdates;

        Trace.Write($"Registered OnAwake count: {_onAwakes.Length}");
        foreach (var onAwake in _onAwakes)
        {
            Trace.Write($"Type: {onAwake.GetType()}");
        }

        Trace.Write($"Registered OnStart count: {_onStarts.Length}");
        foreach (var onStart in _onStarts)
        {
            Trace.Write($"Type: {onStart.GetType()}");
        }

        Trace.Write($"Registered OnEnable count: {_onEnables.Length}");
        foreach (var onEnable in _onEnables)
        {
            Trace.Write($"Type: {onEnable.GetType()}");
        }

        Trace.Write($"Registered OnPreUpdate count: {_onPreUpdates.Length}");
        foreach (var onPreUpdate in _onPreUpdates)
        {
            Trace.Write($"Type: {onPreUpdate.GetType()}");
        }

        Trace.Write($"Registered OnUpdate count: {_onUpdates.Length}");
        foreach (var onUpdate in _onUpdates)
        {
            Trace.Write($"Type: {onUpdate.GetType()}");
        }

        Trace.Write($"Registered OnFixedUpdate count: {_onFixedUpdates.Length}");
        foreach (var onFixedUpdate in _onFixedUpdates)
        {
            Trace.Write($"Type: {onFixedUpdate.GetType()}");
        }
    }

    // calls awake before any other script
    public void Awake()
    {
        foreach (var onAwake in _onAwakes)
        {
            onAwake.Awake();
        }
    }

    // calls onEnable before any other script
    public void OnEnable()
    {
        foreach (var onEnable in _onEnables)
        {
            onEnable.OnEnable();
        }
    }

    // calls start before any other script
    public void Start()
    {
        foreach (var onStart in _onStarts)
        {
            onStart.Start();
        }
    }

    public void Update()
    {
        if (_updated) return;
        _updated = true;

        CallOnPreUpdate();

        foreach (var update in _onUpdates)
        {
            update.Update();
        }

        //Overlay.Update();
        //GameCapture.Update();
    }

    // right now I don't call this update before other scripts so I don't need to check if it was already called
    public void LateUpdate()
    {
        _updated = false;
        _calledPreUpdate = false;
    }

    public void PreFixedUpdate()
    {
        CallOnPreUpdate();
    }

    // right now I don't call this update before other scripts so I don't need to check if it was already called
    public void FixedUpdate()
    {
        foreach (var update in _onFixedUpdates)
        {
            update.FixedUpdate();
        }
    }

    private void CallOnPreUpdate()
    {
        if (_calledPreUpdate) return;
        _calledPreUpdate = true;

        foreach (var onPreUpdate in _onPreUpdates)
        {
            onPreUpdate.PreUpdate();
        }
    }
}