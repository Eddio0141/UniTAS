using System;
using System.Linq;
using HarmonyLib;
using UniTAS.Patcher.Extensions;

namespace UniTAS.Patcher.Utils;

public static class AccessToolsUtils
{
    public static Type FindTypeFully(string type)
    {
        var targetType = AccessTools.TypeByName(type);
        if (targetType != null) return targetType;

        // could be somewhere else, and TypeByName might not be able to find it
        return AccessTools.AllAssemblies()
            .SelectMany(AccessTools.GetTypesFromAssembly).FirstOrDefault(x => x.SaneFullName() == type);
    }
}