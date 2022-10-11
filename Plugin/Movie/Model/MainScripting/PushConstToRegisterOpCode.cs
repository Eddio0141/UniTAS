using System.Collections.Generic;
using UniTASPlugin.Movie.Model.MainScripting.ValueTypes;

namespace UniTASPlugin.Movie.Model.MainScripting;

public class PushConstToRegisterOpCode : OpCodeBase, IPushRegister
{
    private readonly RegisterType _registerType;
    private readonly IValueType _value;

    public KeyValuePair<RegisterType, IValueType> GetPushedValue()
    {
        return new KeyValuePair<RegisterType, IValueType>(_registerType, _value);
    }
}