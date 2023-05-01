using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;

namespace UniTAS.Plugin.Interfaces.UnregisterEventsOnRestart;

public abstract class EventsClearer : IOnPreGameRestart
{
    protected abstract IEnumerable<FieldInfo> FieldsToClear();
    private FieldInfo[] _fieldsToClear;

    public void OnPreGameRestart()
    {
        _fieldsToClear ??= FieldsToClear().ToArray();

        foreach (var field in _fieldsToClear)
        {
            field.SetValue(null, null);
        }
    }
}