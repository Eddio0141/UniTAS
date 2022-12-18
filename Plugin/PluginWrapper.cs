namespace UniTASPlugin;

// ReSharper disable once ClassNeverInstantiated.Global
public class PluginWrapper
{
    private bool _updated;
    private bool _updateTest = true;

    public void Update()
    {
        if (_updateTest)
        {
            Plugin.Log.LogDebug("Update in PluginWrapper");
            _updateTest = false;
        }
        
        if (_updated) return;
        _updated = true;
    }

    public void OnGUI()
    {
        _updated = false;
    }
}