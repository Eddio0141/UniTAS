using HarmonyLib;
using UnityEngine;

namespace UniTASPlugin.VersionSafeWrapper;

internal class RandomWrap
{
    public static void InitState(int seed)
    {
        var initState = Traverse.Create(typeof(Random)).Method("InitState", new System.Type[] { typeof(int) });

        if (initState.MethodExists())
        {
            initState.GetValue(seed);
            return;
        }
        throw new System.Exception("Random init state fallback not written TODO");
    }
}
