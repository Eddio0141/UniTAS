using System.Diagnostics.CodeAnalysis;
using StructureMap;
using UniTAS.Plugin.Models.Movie;
using UniTAS.Plugin.Services.Movie;

namespace UniTAS.Plugin.Tests.MovieRunner;

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