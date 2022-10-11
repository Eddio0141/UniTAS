using System.Collections.Generic;
using UniTASPlugin.Movie.Model.MainScripting.ValueTypes;

namespace UniTASPlugin.Movie.Model.MainScripting;

/// <summary>
/// Pushes a value to a register.
/// For example, arguments can have N number of arguments, so you can keep pushing argument values for a method to use.
/// </summary>
public interface IPushRegister
{
    KeyValuePair<RegisterType, IValueType> GetPushedValue();
}