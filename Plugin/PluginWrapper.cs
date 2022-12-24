using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.Interfaces.StartEvent;
using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.Patches.PatchProcessor;

namespace UniTASPlugin;

// ReSharper disable once ClassNeverInstantiated.Global
public class PluginWrapper
{
    private bool _updated;
    private bool _calledPreUpdate;

    private readonly IOnUpdate[] _onUpdates;
    private readonly IOnFixedUpdate[] _onFixedUpdates;
    private readonly IOnAwake[] _onAwakes;
    private readonly IOnStart[] _onStarts;
    private readonly IOnEnable[] _onEnables;
    private readonly IOnPreUpdates[] _onPreUpdates;

    public PluginWrapper(IEnumerable<IOnUpdate> onUpdates, IEnumerable<IOnFixedUpdate> onFixedUpdates,
        IEnumerable<IOnAwake> onAwakes, IEnumerable<IOnStart> onStarts, IEnumerable<IOnEnable> onEnables,
        IEnumerable<IOnPreUpdates> onPreUpdates,
        IEnumerable<PatchProcessor> patchProcessors)
    {
        _onFixedUpdates = onFixedUpdates.ToArray();
        _onAwakes = onAwakes.ToArray();
        _onStarts = onStarts.ToArray();
        _onEnables = onEnables.ToArray();
        _onPreUpdates = onPreUpdates.ToArray();
        _onUpdates = onUpdates.ToArray();

        foreach (var patchProcessor in patchProcessors)
        {
            patchProcessor.ProcessModules();
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
        GameTracker.LateUpdate();
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
        if (!_calledPreUpdate)
        {
            _calledPreUpdate = true;
            foreach (var onPreUpdate in _onPreUpdates)
            {
                onPreUpdate.PreUpdate();
            }
        }
    }
}