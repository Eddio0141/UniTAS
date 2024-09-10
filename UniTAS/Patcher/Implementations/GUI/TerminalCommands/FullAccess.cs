using System;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop.RegistrationPolicies;
using UniTAS.Patcher.Interfaces.GUI;

namespace UniTAS.Patcher.Implementations.GUI.TerminalCommands;

public class FullAccess : TerminalCmd
{
    public override string Name => "full_access";

    public override string Description =>
        "Removes or applies sandbox limits for lua scripts" +
        "\n  !!!IF RUNNING TAS MOVIES, DISABLE FULL ACCESS OR MAKE SURE THE MOVIE CAN BE 100% TRUSTED!!!" +
        "\n  (though I'm not sure if this will actually be a risk since you still need to allow access to those types from the C# side" +
        "\n\n  Arg0 (bool): allow full access to types";

    public override Delegate Callback => Execute;

    private IRegistrationPolicy _originalPolicy;

    private void Execute(Script script, bool fullAccess)
    {
        var print = script.Options.DebugPrint;
        if (fullAccess)
        {
            _originalPolicy = UserData.RegistrationPolicy;
            UserData.RegistrationPolicy = new AutomaticRegistrationPolicy();
            print(
                "Full access to types enabled, !!!IF RUNNING TAS MOVIES, DISABLE FULL ACCESS OR MAKE SURE THE MOVIE CAN BE 100% TRUSTED JUST IN CASE!!!");
        }
        else
        {
            UserData.RegistrationPolicy = _originalPolicy;
            print("Restored original access to types");
        }
    }
}