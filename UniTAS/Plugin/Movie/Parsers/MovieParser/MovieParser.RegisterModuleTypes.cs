using System;
using MoonSharp.Interpreter;
using UniTAS.Plugin.Movie.Engine;

namespace UniTAS.Plugin.Movie.Parsers.MovieParser;

public partial class MovieParser
{
    private readonly Type[] _moduleTypes =
    {
        typeof(EngineMethods.Implementations.Movie)
    };

    private void RegisterModuleTypes(IMovieEngine engine)
    {
        var globalTable = engine.Script.Globals;

        foreach (var type in _moduleTypes)
        {
            globalTable.RegisterModuleType(type);
        }
    }
}