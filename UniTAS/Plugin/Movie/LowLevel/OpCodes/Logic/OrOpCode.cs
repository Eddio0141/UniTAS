using UniTAS.Plugin.Movie.LowLevel.Register;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes.Logic;

public class OrOpCode : LogicComparison
{
    public OrOpCode(RegisterType dest, RegisterType left, RegisterType right) : base(dest, left, right)
    {
    }
}