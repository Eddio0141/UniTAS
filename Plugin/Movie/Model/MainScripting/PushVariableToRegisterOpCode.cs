using System.Collections.Generic;

namespace UniTASPlugin.Movie.Model.MainScripting;

public class PushVariableToRegisterOpCode : OpCodeBase
{
    public RegisterType RegisterType { get; }
    public string Name { get; }
}