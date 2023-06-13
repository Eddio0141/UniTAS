using HarmonyLib;
using UniTAS.Patcher.Interfaces.Movie;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.Movie.Engine.Modules;

public class Unity : EngineMethodClass
{
    public Object[] find_objects_by_type(string typeName, bool includeInactive = false)
    {
        var type = AccessTools.TypeByName(typeName);
        if (type == null)
        {
            return null;
        }

        return includeInactive ? Resources.FindObjectsOfTypeAll(type) : Object.FindObjectsOfType(type);
    }
}