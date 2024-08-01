using System;
using System.Globalization;
using System.Linq;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GUI;

namespace UniTAS.Patcher.Implementations.GUI.TerminalCommands;

public class Restart : TerminalCmd
{
    private readonly IGameRestart _gameRestart;

    public Restart(IGameRestart gameRestart)
    {
        _gameRestart = gameRestart;
    }

    public override string Command => "restart";
    public override string Description => "soft restarts the game";

    public override bool Execute(string[] args, ITerminalWindow terminalWindow)
    {
        if (args.Length < 1 || !DateTime.TryParse(args.First(), CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal, out var restartTime))
        {
            restartTime = DateTime.Now;
        }

        _gameRestart.SoftRestart(restartTime);
        return false;
    }
}
