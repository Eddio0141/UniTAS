using System;
using System.Globalization;
using System.Linq;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GUI;

namespace UniTAS.Patcher.Implementations.GUI.TerminalCommands;

public class Restart(IGameRestart gameRestart) : TerminalCmd
{
    public override string Name => "restart";
    public override string Description => "soft restarts the game";

    public override Delegate Callback => Execute;

    private void Execute(string time)
    {
        if (!DateTime.TryParse(time, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal,
                out var restartTime))
        {
            restartTime = DateTime.Now;
        }

        gameRestart.SoftRestart(restartTime);
    }
}