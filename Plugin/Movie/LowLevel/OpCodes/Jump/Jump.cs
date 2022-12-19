namespace UniTASPlugin.Movie.MovieRunner.LowLevel.OpCodes.Jump;

public abstract class Jump : OpCode
{
    public int Offset { get; set; }

    protected Jump(int offset)
    {
        Offset = offset;
    }
}