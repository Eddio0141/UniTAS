using MoonSharp.Interpreter;

namespace UniTAS.Patcher.Implementations.Movie.Engine;

[MoonSharpUserData]
public class ConcurrentIdentifier(bool postUpdate)
{
    public bool PostUpdate { get; } = postUpdate;
}