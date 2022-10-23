using UniTASPlugin.Movie.Models.Script;

namespace UniTASPlugin.Movie.ScriptEngine.EngineInterfaces;

public interface IScriptEngineInitScript
{
    void Init(ScriptModel script);
}