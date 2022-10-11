using System.Collections.Generic;
using UniTASPlugin.Movie.Model.MainScripting.ValueTypes;

namespace UniTASPlugin.Movie.Model.MainScripting;

public class PushConstToRegisterOpCode : OpCodeBase
{
    public RegisterType RegisterType { get; }
    public IValueType Value { get; }
}