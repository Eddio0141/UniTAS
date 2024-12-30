using System;
using System.Collections.Generic;
using System.Linq;

namespace UniTAS.Patcher.ManualServices;

public static class GameInfoManual
{
    public static bool NoGraphics { get; } =
        Environment.GetCommandLineArgs().Any(x => x is "-batchmode" or "-nographics");

    public static Dictionary<string, List<string>> MonoBehaviourWithIEnumerator { get; } = new();
}