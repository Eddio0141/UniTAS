using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine.OpCodes;

public class CastOpCode : OpCodeBase
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