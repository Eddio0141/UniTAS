using StructureMap;
using UniTASPlugin.FixedUpdateSync;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.GameEnvironment.InnerState;
using UniTASPlugin.GameRestart;
using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.Movie;
using UniTASPlugin.Movie.EngineMethods;
using UniTASPlugin.ReverseInvoker;

namespace UniTASPlugin;

public static class ContainerRegister
{
    public static Container Init()
    {
        var container = new Container(c =>
        {
            c.Scan(scanner =>
            {
                scanner.TheCallingAssembly();
                scanner.WithDefaultConventions();

                // scanner.AddAllTypesOf(typeof(IOnUpdate));
                // scanner.AddAllTypesOf(typeof(IOnFixedUpdate));
                scanner.AddAllTypesOf(typeof(EngineExternalMethod));
            });

            c.For<PluginWrapper>().Singleton();

            c.For<IGameRestart>().Singleton().Use<GameRestart.GameRestart>();

            c.For<SyncFixedUpdate>().Singleton();
            c.For<IOnFixedUpdate>().Use(x => x.GetInstance<SyncFixedUpdate>());
            c.For<ISyncFixedUpdate>().Use(x => x.GetInstance<SyncFixedUpdate>());
            c.For<IOnUpdate>().Use(x => x.GetInstance<SyncFixedUpdate>());

            c.For<IPatchReverseInvoker>().Singleton().Use<PatchReverseInvoker>();

            c.For<IMovieRunner>().Singleton().Use<MovieRunner>();

            c.For<VirtualEnvironment>().Singleton();

            c.For<IOnUpdate>().Singleton().Use<GameTime>();

            c.For<IOnUpdate>().Use<VirtualEnvironmentApplier>();
        });

        return container;
    }
}