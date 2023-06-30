using MoonSharp.Interpreter;

namespace UniTAS.Patcher.Services;

public interface ILiveScripting
{
    /// <summary>
    /// An instance of the <see cref="Script"/> class that is set up for scripting.
    /// </summary>
    Script NewScript();
}