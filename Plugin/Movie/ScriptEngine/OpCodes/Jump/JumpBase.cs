namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Jump;

public abstract class JumpBase : OpCodeBase
{
    public int Offset { get; set; }

    protected JumpBase(int offset)
    {
        Offset = offset;
    }
}