using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.Interfaces.StartEvent;
using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.Movie;

namespace UniTASPlugin;

// ReSharper disable once ClassNeverInstantiated.Global
public class PluginWrapper
{
    private bool _updated;

    private readonly IMovieRunner _movieRunner;
    private readonly IOnUpdate[] _onUpdates;
    private readonly IOnFixedUpdate[] _onFixedUpdates;
    private readonly IOnAwake[] _onAwakes;
    private readonly IOnStart[] _onStarts;
    private readonly IOnEnable[] _onEnables;

    public PluginWrapper(IMovieRunner movieRunner, IEnumerable<IOnUpdate> onUpdates, IOnFixedUpdate[] onFixedUpdates,
        IOnAwake[] onAwakes, IOnStart[] onStarts, IOnEnable[] onEnables)
    {
        _movieRunner = movieRunner;
        _onFixedUpdates = onFixedUpdates;
        _onAwakes = onAwakes;
        _onStarts = onStarts;
        _onEnables = onEnables;
        _onUpdates = onUpdates.ToArray();
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

        _movieRunner.Update();
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
        GameTracker.LateUpdate();
    }

    // right now I don't call this update before other scripts so I don't need to check if it was already called
    public void FixedUpdate()
    {
        foreach (var update in _onFixedUpdates)
        {
            update.FixedUpdate();
        }
    }
}