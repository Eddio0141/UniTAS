using UniTASPlugin.Movie.LowLevel.Register;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.LowLevel.OpCodes;

public class CastOpCode : OpCode
{
    public BasicValueType ValueType { get; }
    public RegisterType Source { get; }
    public RegisterType Dest { get; }

    public CastOpCode(BasicValueType valueType, RegisterType source, RegisterType dest)
    {
        ValueType = valueType;
        Source = source;
        Dest = dest;
    }
}