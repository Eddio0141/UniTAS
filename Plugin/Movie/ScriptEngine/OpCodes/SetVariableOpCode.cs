﻿namespace UniTASPlugin.Movie.ScriptEngine.OpCodes;

public class SetVariableOpCode : OpCodeBase
{
    public string Name { get; }
    public RegisterType Register { get; }

    public SetVariableOpCode(RegisterType register, string name)
    {
        Register = register;
        Name = name;
    }
}