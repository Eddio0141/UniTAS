using Ninject.Modules;
using UniTASPlugin.Movie;
using UniTASPlugin.Movie.ScriptEngine;
using UniTASPlugin.Movie.ScriptEngine.EngineMethods;
using UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;
using UniTASPlugin.Movie.ScriptEngine.Parsers;
using UniTASPlugin.Movie.ScriptEngine.Parsers.MoviePropertiesParser;
using UniTASPlugin.Movie.ScriptEngine.Parsers.MovieScriptParser;

namespace UniTASPlugin.NInjectModules;

public class MovieEngineModule : NinjectModule
{
    public override void Load()
    {
        Bind<IMoviePropertyParser>().To<DefaultMoviePropertiesParser>();
        Bind<IMovieScriptParser>().To<DefaultMovieScriptParser>();
        Bind<IMovieSectionSplitter>().To<DefaultMovieSectionSplitter>();
        Bind<IMovieParser>().To<ScriptEngineMovieParser>();

        Bind<IMovieRunner>().To<ScriptEngineMovieRunner>();
        Bind<ScriptEngineMovieRunner>().ToSelf().InSingletonScope();
    }
}