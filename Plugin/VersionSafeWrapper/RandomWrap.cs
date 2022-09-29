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
            _ = initState.GetValue(seed);
            return;
        }
        // TODO does this work?
        Random.seed = seed;
    }
}
