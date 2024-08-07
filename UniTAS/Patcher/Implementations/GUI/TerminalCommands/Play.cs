using System;
using System.IO;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Movie;

namespace UniTAS.Patcher.Implementations.GUI.TerminalCommands;

public class Play(IMovieRunner movieRunner, ILogger logger) : TerminalCmd
{
    public override string Name => "play";

    public override string Description =>
        "Plays back a movie. Arg0: path to movie script, absolute path or relative path (to the game executable)";

    public override Delegate Callback => Execute;

    private void Execute(Script script, string path)
    {
        var print = script.Options.DebugPrint;

        string absPath;
        try
        {
            absPath = Path.GetFullPath(path);
        }
        catch (Exception e)
        {
            print($"Failed to get absolute path from arg0: {e.Message}");
            return;
        }

        string movieContent;
        try
        {
            movieContent = File.ReadAllText(absPath);
        }
        catch (Exception e)
        {
            print($"Failed to read from path {absPath}: {e.Message}");
            return;
        }

        try
        {
            movieRunner.RunFromInput(movieContent);
        }
        catch (Exception e)
        {
            print($"Failed to start movie: {e.Message}");
            logger.LogDebug($"Movie start failed from command: {e}");
        }
    }
}