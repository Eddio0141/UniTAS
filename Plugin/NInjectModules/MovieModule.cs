using Ninject.Modules;
using UniTASPlugin.Movie;
using UniTASPlugin.Movie.DefaultParsers;
using UniTASPlugin.Movie.DefaultParsers.DefaultMoviePropertiesParser;
using UniTASPlugin.Movie.DefaultParsers.DefaultMovieScriptParser;
using UniTASPlugin.Movie.ParseInterfaces;

namespace UniTASPlugin.NInjectModules;

public class MovieModule : NinjectModule
{
    public override void Load()
    {
        Bind<IMoviePropertyParser>().To<DefaultMoviePropertiesParser>();
        Bind<IMovieScriptParser>().To<DefaultMovieScriptParser>();
        Bind<IMovieSectionSplitter>().To<DefaultMovieSectionSplitter>();
        Bind<IMovieParser>().To<DefaultMovieParser>();

        Bind<IMovieRunner>().To<DefaultMovieRunner>();
        Bind<DefaultMovieRunner>().ToSelf().InSingletonScope();
    }
}