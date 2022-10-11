using UniTASPlugin.Movie.Model.Main.ValueTypes;

namespace UniTASPlugin.Movie.Model.Main;

public class MethodModel<TArg, TRet> : ScopeModel<TRet>
where TArg : IValueType
where TRet : IValueType
{
    public string Name { get; }
    public VariableModel<TArg>[] Arguments { get; }
}