using System;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations.GUI.TerminalCommands;

public class Service : TerminalCmd
{
    public override string Name => "service";

    public override string Description =>
        "Gets a service from the dependency injection kernel\n  Arg0 (string): type by it's name, Arg1 (bool: false): get all instances";

    public override Delegate Callback => Execute;

    private static DynValue Execute(Script script, string typeRaw, bool getAll = false)
    {
        var type = AccessToolsUtils.FindTypeFully(typeRaw);
        if (getAll)
        {
            var instances = ContainerStarter.Kernel.GetAllInstances(type);
            return DynValue.FromObject(script, instances);
        }

        var instance = ContainerStarter.Kernel.GetInstance(type);
        return DynValue.FromObject(script, instance);
    }
}