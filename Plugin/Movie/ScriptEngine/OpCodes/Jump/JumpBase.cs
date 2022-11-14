namespace UniTASPlugin.Movie.ScriptEngine.OpCodes.Jump;

public abstract class JumpBase : OpCodeBase
{
    public int Offset { get; }

    protected JumpBase(int offset)
    {
        Offset = offset;
    }
}