using UniTAS.Plugin.Movie.LowLevel.Register;
using UniTAS.Plugin.Movie.ValueTypes;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes;

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