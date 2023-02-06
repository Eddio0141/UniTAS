using UniTASPlugin.Movie.LowLevel.Register;

namespace UniTASPlugin.Movie.LowLevel.OpCodes.Jump;

public class JumpIfFalse : Jump
{
    public RegisterType Register { get; }

    public JumpIfFalse(int offset, RegisterType register) : base(offset)
    {
        Register = register;
    }
}