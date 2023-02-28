using BepInEx.Logging;
using StructureMap;
using UniTAS.Plugin.Logger;
using UniTAS.Plugin.Movie.Engine;
using UniTAS.Plugin.Movie.MovieModels.Properties;
using UniTAS.Plugin.Movie.Parsers.MovieParser;

namespace UniTAS.Plugin.Tests.MovieRunner;

public class Utils
{
    public class DummyLogger : IMovieLogger
    {
        public List<string> Infos { get; } = new();
        public List<string> Warns { get; } = new();

        public void LogError(object data)
        {
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
        var kernel = ContainerRegister.Init();

        kernel.Configure(x =>
        {
            x.ForSingletonOf<Utils.DummyLogger>().Use<Utils.DummyLogger>();
            x.For<IMovieLogger>().Use(y => y.GetInstance<Utils.DummyLogger>());
        });

        var parser = kernel.GetInstance<IMovieParser>();
        var parsed = parser.Parse(input);

        return (parsed.Item1, parsed.Item2, kernel);
    }
}