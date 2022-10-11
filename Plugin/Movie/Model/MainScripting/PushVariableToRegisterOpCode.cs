using System.Collections.Generic;

namespace UniTASPlugin.Movie.Model.MainScripting;

public class PushVariableToRegisterOpCode : OpCodeBase, IPushRegister
{
    private readonly RegisterType _registerType;
    private readonly string _name;

    public KeyValuePair<RegisterType, string> GetPushedValue()
    {
        return new KeyValuePair<RegisterType, string>(_registerType, _name);
    }
}