using UniTASPlugin.Movie.Model.Main.ValueTypes;

namespace UniTASPlugin.Movie.Model.Main;

public interface IReturnable<out T>
where T : IValueType
{
    T GetReturn();
}