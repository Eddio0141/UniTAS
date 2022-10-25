using Ninject.Modules;
using UniTASPlugin.Movie;
using UniTASPlugin.Movie.DefaultParsers;
using UniTASPlugin.Movie.DefaultParsers.DefaultMoviePropertiesParser;
using UniTASPlugin.Movie.ParseInterfaces;
using UniTASPlugin.Movie.ScriptEngine;
using UniTASPlugin.Movie.ScriptEngine.EngineInterfaces;

namespace UniTASPlugin.NInjectModules;

public class MovieModule : NinjectModule
{
    public override void Load()
    {
        Bind<IMoviePropertyParser>().To<DefaultMoviePropertiesParser>();
        Bind<IMovieScriptParser>().To<DefaultMovieScriptParser>();
        Bind<IMovieSectionSplitter>().To<DefaultMovieSectionSplitter>();
        Bind<IMovieParser>().To<MovieParseProcessor>();

        Bind<IScriptEngineInitScript>().To<MovieScriptEngine>();
        Bind<IScriptEngineMovieEnd>().To<MovieScriptEngine>();
        Bind<IScriptEngineCurrentState>().To<MovieScriptEngine>();
        Bind<IScriptEngineAdvanceFrame>().To<MovieScriptEngine>();

        Bind(typeof(MovieRunner<>)).ToSelf().InSingletonScope();
    }
}