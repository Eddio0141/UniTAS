using System.IO;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Movie;

namespace UniTAS.Patcher.Implementations.GUI.TerminalCommands;

public class Play(IMovieRunner movieRunner, ILogger logger) : TerminalCmd
{
    public override string Command => "play";

    public override string Description => "Plays back a movie. Arg0: path to movie script, absolute path or relative path (to the game executable)";

    public override bool Execute(string[] args, ITerminalWindow terminalWindow)
    {
        if (args.Length != 1)
        {
            terminalWindow.TerminalPrintLine("Missing arg 0. Run `help play` for more info");
            return false;
        }

        string absPath;
        try
        {
            absPath = Path.GetFullPath(args[0]);
        }
        catch (System.Exception e)
        {
            terminalWindow.TerminalPrintLine($"Failed to get absolute path from arg0: {e.Message}");
            return false;
        }

        string movieContent;
        try
        {
            movieContent = File.ReadAllText(absPath);
        }
        catch (System.Exception e)
        {
            terminalWindow.TerminalPrintLine($"Failed to read from path {absPath}: {e.Message}");
            return false;
        }

        try
        {
            movieRunner.RunFromInput(movieContent);
        }
        catch (System.Exception e)
        {
            terminalWindow.TerminalPrintLine($"Failed to start movie: {e.Message}");
            logger.LogDebug($"Movie start failed from command: {e}");
        }

        return false;
    }
}