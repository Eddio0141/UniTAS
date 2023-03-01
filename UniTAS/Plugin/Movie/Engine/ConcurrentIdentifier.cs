using MoonSharp.Interpreter;

namespace UniTAS.Plugin.Movie.Engine;

[MoonSharpUserData]
public class ConcurrentIdentifier
{
    public int Index { get; set; }
    public bool PreUpdate { get; }

    public ConcurrentIdentifier(int index, bool preUpdate)
    {
        Index = index;
        PreUpdate = preUpdate;
    }
}