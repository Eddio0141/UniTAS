using UniTAS.Plugin.Movie.LowLevel.Register;

namespace UniTAS.Plugin.Movie.LowLevel.OpCodes.Logic;

public class NotOpCode : Logic
{
    public RegisterType Source { get; }

    public NotOpCode(RegisterType dest, RegisterType source) : base(dest)
    {
        Source = source;
    }
}