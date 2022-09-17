﻿using HarmonyLib;
using System;

namespace v2021_2_14.Patches.Time.__System;

[HarmonyPatch(typeof(DateTime), nameof(DateTime.Now), MethodType.Getter)]
class NowGetter
{
    static bool Prefix(ref DateTime __result)
    {
        __result = Core.TAS.Main.Time;
        return false;
    }
}
