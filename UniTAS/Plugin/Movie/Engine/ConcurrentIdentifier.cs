using MoonSharp.Interpreter;

namespace UniTAS.Plugin.Movie.Engine;

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
}