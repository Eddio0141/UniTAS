using System;
using HarmonyLib;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations.GUI.TerminalCommands;

public class TraverseCmd : TerminalCmd
{
    public override string Name => "traverse";
    public override string Description => "traverse API wrapper";
    public override Delegate Callback => Execute;

    private static Traverse Execute(Script script, object arg)
    {
        if (arg is string s)
        {
            // find type
            var type = AccessToolsUtils.FindTypeFully(s);
            if (type == null)
            {
                script.Options.DebugPrint($"failed to find type {s}");
                return null;
            }

            return new Traverse(type);
        }

        return new Traverse(arg);
    }

    public override void Setup()
    {
        base.Setup();

        UserData.RegisterType<Traverse>();
    }
}
