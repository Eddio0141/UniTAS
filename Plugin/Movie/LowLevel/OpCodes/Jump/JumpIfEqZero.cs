using UniTASPlugin.Movie.LowLevel.Register;

namespace UniTASPlugin.Movie.LowLevel.OpCodes.Jump;

public class JumpIfEqZero : Jump
{
    public RegisterType Register { get; }

    public JumpIfEqZero(int offset, RegisterType register) : base(offset)
    {
        Register = register;
    }
}