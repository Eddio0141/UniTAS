using UniTASPlugin.Movie.MovieRunner.LowLevel.Register;
using UniTASPlugin.Movie.MovieRunner.ValueTypes;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes;

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