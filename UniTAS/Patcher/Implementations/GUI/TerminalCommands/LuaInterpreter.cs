using System;
using BepInEx;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GUI;

namespace UniTAS.Patcher.Implementations.GUI.TerminalCommands;

public class LuaInterpreter(ILiveScripting liveScripting) : TerminalCmd
{
    public override string Command => "lua";
    public override string Description => "Lua interpreter to interact with UniTAS and the game";

    private ITerminalWindow _terminalWindow;

    private readonly Script _script = liveScripting.NewScript();

    public override bool Execute(string[] args, ITerminalWindow terminalWindow)
    {
        _terminalWindow = terminalWindow;

        _script.Options.DebugPrint = terminalWindow.TerminalPrintLine;
        _script.Globals["exit"] = (Action)ExitMethod;

        return true;
    }

    public override void OnInput(string input, bool split)
    {
        if (input.IsNullOrWhiteSpace()) return;

        if (split)
        {
            _terminalWindow.TerminalPrintLine($">> {input}");
            return;
        }

        try
        {
            _script.DoString(input);
        }
        catch (InterpreterException e)
        {
            _terminalWindow.TerminalPrintLine(e.Message);
        }
        catch (Exception e)
        {
            _terminalWindow.TerminalPrintLine(e.ToString());
        }
    }

    private void ExitMethod()
    {
        _terminalWindow.TerminalPrintLine("Exiting Lua interpreter...");
        _terminalWindow.ReleaseTerminal();
    }
}