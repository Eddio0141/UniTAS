using System.Diagnostics.CodeAnalysis;
using StructureMap;
using UniTAS.Patcher.Models.Movie;
using UniTAS.Patcher.Services.Movie;

namespace Patcher.Tests.MovieRunner;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public static class Utils
{
    public static (IMovieEngine, PropertiesModel, IContainer) Setup(string input)
    {
        var kernel = KernelUtils.Init();
        var parser = kernel.GetInstance<IMovieParser>();
        var parsed = parser.Parse(input);

        return (parsed.Item1, parsed.Item2, kernel);
    }
}