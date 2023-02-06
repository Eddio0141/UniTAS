namespace UniTASPlugin.Movie.LowLevel.OpCodes.Jump;

public abstract class Jump : OpCode
{
    public int Offset { get; set; }

    protected Jump(int offset)
    {
        Offset = offset;
    }
}