using System;
using System.Linq;

namespace UniTAS.Patcher.ManualServices;

public static class GameInfoManual
{
    public static bool NoGraphics { get; } =
        Environment.GetCommandLineArgs().Any(x => x is "-batchmode" or "-nographics");
}