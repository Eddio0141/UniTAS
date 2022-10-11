using UniTASPlugin.Movie.Model.Main.ValueTypes;

namespace UniTASPlugin.Movie.Model.Main;

public class ScopeModel<TRet> : IReturnable<TRet>
where TRet : IValueType
{
    public TRet GetReturn()
    {
        throw new System.NotImplementedException();
    }
}