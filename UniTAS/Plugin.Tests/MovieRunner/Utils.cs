using System.Diagnostics.CodeAnalysis;
using BepInEx.Logging;
using StructureMap;
using UniTAS.Plugin.Interfaces.Update;
using UniTAS.Plugin.Logger;
using UniTAS.Plugin.Movie;
using UniTAS.Plugin.Movie.Engine;
using UniTAS.Plugin.Movie.EngineMethods;
using UniTAS.Plugin.Movie.MovieModels.Properties;
using UniTAS.Plugin.Movie.Parsers.MovieParser;

namespace UniTAS.Plugin.Tests.MovieRunner;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public static class Utils
{
    public class DummyLogger : IMovieLogger
    {
        public List<string> Infos { get; } = new();
        public List<string> Warns { get; } = new();
        public List<string> Errors { get; } = new();

        public void LogError(object data)
        {
            Errors.Add(data.ToString() ?? string.Empty);
        }

        public void LogInfo(object data)
        {
            Infos.Add(data.ToString() ?? string.Empty);
        }

        public void LogWarning(object data)
        {
            Warns.Add(data.ToString() ?? string.Empty);
        }

#pragma warning disable 67
        public event EventHandler<LogEventArgs>? OnLog;
#pragma warning restore 67
    }

    public static (IMovieEngine, PropertiesModel, IContainer) Setup(string input)
    {
        var kernel = Init();
        var parser = kernel.GetInstance<IMovieParser>();
        var parsed = parser.Parse(input);

        return (parsed.Item1, parsed.Item2, kernel);
    }

    private static IContainer Init()
    {
        return new Container(c =>
        {
            c.Scan(scanner =>
            {
                scanner.AssemblyContainingType<Plugin>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf<EngineMethodClass>();
            });

            c.ForSingletonOf<DummyLogger>().Use<DummyLogger>();
            c.For<IMovieLogger>().Use(y => y.GetInstance<DummyLogger>());

            c.ForSingletonOf<Movie.MovieRunner>().Use<Movie.MovieRunner>();
            c.For<IMovieRunner>().Use(x => x.GetInstance<Movie.MovieRunner>());
            c.For<IOnPreUpdates>().Use(x => x.GetInstance<Movie.MovieRunner>());
        });
    }
}