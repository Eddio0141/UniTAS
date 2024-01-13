using System;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Services.Movie;

namespace UniTAS.Patcher.Implementations.Movie.Parser;

public partial class MovieParser
{
    private readonly Type[] _moduleTypes =
    [
        typeof(Engine.Modules.Movie)
    ];

    private void RegisterModuleTypes(IMovieEngine engine)
    {
        var globalTable = engine.Script.Globals;

        foreach (var type in _moduleTypes)
        {
            globalTable.RegisterModuleType(type);
        }
    }
}