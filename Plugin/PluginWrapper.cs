using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.Movie;

namespace UniTASPlugin;

// ReSharper disable once ClassNeverInstantiated.Global
public class PluginWrapper
{
    private bool _updated;
    private bool _onGUIUpdated;

    private readonly IMovieRunner _movieRunner;
    private readonly IOnUpdate[] _onUpdates;

    public PluginWrapper(IMovieRunner movieRunner, IEnumerable<IOnUpdate> onUpdates)
    {
        _movieRunner = movieRunner;
        _onUpdates = onUpdates.ToArray();
    }

    public void Update()
    {
        if (_updated) return;
        _updated = true;

        _onGUIUpdated = false;

        _movieRunner.Update();
        foreach (var update in _onUpdates)
        {
            update.Update();
        }

        //Overlay.Update();
        //GameCapture.Update();
    }

    public void OnGUI()
    {
        if (_onGUIUpdated) return;
        _onGUIUpdated = true;

        _updated = false;
    }
}