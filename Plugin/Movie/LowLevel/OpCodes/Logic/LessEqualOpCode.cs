using UniTASPlugin.Movie.MovieRunner.LowLevel.Register;

namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.Logic;

public class LessEqualOpCode : LogicComparison
{
    public LessEqualOpCode(RegisterType dest, RegisterType left, RegisterType right) : base(dest, left, right)
    {
    }
}