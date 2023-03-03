using MoonSharp.Interpreter;
using UniTAS.Plugin.Movie.Engine;

namespace UniTAS.Plugin.Movie.Parsers.MovieParser;

public partial class MovieParser
{
    private static void RegisterModuleTypes(IMovieEngine engine)
    {
        var globalTable = engine.Script.Globals;

        globalTable.RegisterModuleType<EngineMethods.Implementations.Movie>();
    }
}