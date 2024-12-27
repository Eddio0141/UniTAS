using System;
using System.Reflection;
using UniTAS.Patcher.Extensions;

namespace UniTAS.Patcher.Utils;

public static class PatchHelper
{
    public static Exception CleanupIgnoreFail(MethodBase original, Exception ex)
    {
        if (ex != null)
        {
            StaticLogger.Log.LogDebug(original == null
                ? $"Failed to patch, exception: {ex}"
                : $"Failed to patch {original.DeclaringType.SaneFullName()}: {original}, exception: {ex}");
        }

        return null;
    }
}