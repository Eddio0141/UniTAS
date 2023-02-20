using UniTAS.Plugin.Movie.LowLevel.Register;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes.Logic;

public class GreaterOpCode : LogicComparison
{
    public GreaterOpCode(RegisterType dest, RegisterType left, RegisterType right) : base(dest, left, right)
    {
    }
}