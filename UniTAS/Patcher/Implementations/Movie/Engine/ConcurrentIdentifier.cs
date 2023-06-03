using MoonSharp.Interpreter;

namespace UniTAS.Patcher.Implementations.Movie.Engine;

[MoonSharpUserData]
public class ConcurrentIdentifier
{
    public int Index { get; set; }
    public bool PostUpdate { get; }

    public ConcurrentIdentifier(int index, bool postUpdate)
    {
        Index = index;
        PostUpdate = postUpdate;
    }

    private bool Equals(ConcurrentIdentifier other)
    {
        return Index == other.Index && PostUpdate == other.PostUpdate;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ConcurrentIdentifier)obj);
    }

    public override int GetHashCode()
    {
        return PostUpdate.GetHashCode();
    }
}