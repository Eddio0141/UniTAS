using UniTAS.Plugin.Movie.LowLevel.Register;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes.Logic;

public class NotEqualOpCode : LogicComparison
{
    public NotEqualOpCode(RegisterType dest, RegisterType left, RegisterType right) : base(dest, left, right)
    {
    }
}