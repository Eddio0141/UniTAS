using System;
using UniTAS.Plugin.Interfaces.StartEvent;
using UniTAS.Plugin.Interfaces.Update;

namespace UniTAS.Plugin.Interfaces;

// ReSharper disable once ClassNeverInstantiated.Global
public class MonoBehEventInvoker : IMonoBehEventInvoker, IUpdateEvents
{
    private bool _updated;
    private bool _calledFixedUpdate;
    private bool _calledPreUpdate;

    private readonly IOnAwake[] _onAwakes;
    private readonly IOnStart[] _onStarts;
    private readonly IOnEnable[] _onEnables;
    private readonly IOnPreUpdates[] _onPreUpdates;
    private readonly IOnUpdate[] _onUpdates;
    private readonly IOnFixedUpdate[] _onFixedUpdates;
    private readonly IOnGUI[] _onGUIs;
    private readonly IOnPostRender[] _onPostRenders;

    public MonoBehEventInvoker(IOnAwake[] onAwakes, IOnStart[] onStarts, IOnEnable[] onEnables,
        IOnPreUpdates[] onPreUpdates, IOnUpdate[] onUpdates, IOnFixedUpdate[] onFixedUpdates, IOnGUI[] onGUIs,
        IOnPostRender[] onPostRenders)
    {
        _onAwakes = onAwakes;
        _onStarts = onStarts;
        _onEnables = onEnables;
        _onPreUpdates = onPreUpdates;
        _onUpdates = onUpdates;
        _onFixedUpdates = onFixedUpdates;
        _onGUIs = onGUIs;
        _onPostRenders = onPostRenders;
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

        _calledFixedUpdate = false;

        CallOnPreUpdate();

        foreach (var update in _onUpdates)
        {
            update.Update();
        }
    }

    // right now I don't call this update before other scripts so I don't need to check if it was already called
    public void LateUpdate()
    {
        _updated = false;
        _calledPreUpdate = false;
    }

    public void FixedUpdate()
    {
        if (_calledFixedUpdate) return;
        _calledFixedUpdate = true;

        CallOnPreUpdate();

        foreach (var update in _onFixedUpdates)
        {
            update.FixedUpdate();
        }
    }

    public void OnGUI()
    {
        // currently, this doesn't get called before other scripts
        foreach (var onGui in _onGUIs)
        {
            onGui.OnGUI();
        }

        OnGUIEvent?.Invoke();
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

    public void OnPostRender()
    {
        foreach (var onPostRender in _onPostRenders)
        {
            onPostRender.OnPostRender();
        }
    }

    public event Action OnGUIEvent;
}