using UniTAS.Plugin.Movie.LowLevel.Register;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes.Logic;

public class AndOpCode : LogicComparison
{
    public AndOpCode(RegisterType dest, RegisterType left, RegisterType right) : base(dest, left, right)
    {
    }
}