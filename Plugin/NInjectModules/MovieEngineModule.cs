using Ninject.Modules;
using UniTASPlugin.Movie;
using UniTASPlugin.Movie.ScriptEngine;
using UniTASPlugin.Movie.ScriptEngine.EngineMethods;
using UniTASPlugin.Movie.ScriptEngine.EngineMethods.Exceptions;
using UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;
using UniTASPlugin.Movie.ScriptEngine.Parsers;
using UniTASPlugin.Movie.ScriptEngine.Parsers.MoviePropertiesParser;
using UniTASPlugin.Movie.ScriptEngine.Parsers.MovieScriptParser;

namespace UniTASPlugin.NInjectModules;

public class MovieEngineModule : NinjectModule
{
    public override void Load()
    {
        // parser binds
        Bind<IMoviePropertyParser>().To<DefaultMoviePropertiesParser>();
        Bind<IMovieScriptParser>().To<DefaultMovieScriptParser>();
        Bind<IMovieSectionSplitter>().To<DefaultMovieSectionSplitter>();
        Bind<IMovieParser>().To<ScriptEngineMovieParser>();

        // runner binds
        Bind<IMovieRunner>().To<ScriptEngineMovieRunner>();
        Bind<ScriptEngineMovieRunner>().ToSelf().InSingletonScope();

        // extern method binds
        Bind<EngineExternalMethod>().To<PrintExternalMethod>();

        Bind<EngineExternalMethod>().To<RegisterExternalMethod>();
        Bind<EngineExternalMethod>().To<UnregisterExternalMethod>();

        Bind<EngineExternalMethod>().To<HoldKeyExternalMethod>();
        Bind<EngineExternalMethod>().To<UnHoldKeyExternalMethod>();
        Bind<EngineExternalMethod>().To<ClearHeldKeysExternalMethod>();

        Bind<EngineExternalMethod>().To<MoveMouseExternalMethod>();
    }
}