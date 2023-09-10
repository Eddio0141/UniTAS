using System.Linq;
using MoonSharp.Interpreter;
using StructureMap.Pipeline;
using UniTAS.Patcher.Implementations.VirtualEnvironment;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.DontRunIfPaused;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Movie;

namespace Patcher.Tests.Kernel;

public class KernelTests
{
    [Fact]
    public void FfmpegFactory()
    {
        var kernel = KernelUtils.Init();

        var ffmpegProcessFactory = kernel.GetInstance<IFfmpegProcessFactory>();
        Assert.NotNull(ffmpegProcessFactory);

        var ffmpegProcessFactory2 = kernel.GetInstance<IFfmpegProcessFactory>();
        Assert.NotNull(ffmpegProcessFactory2);

        Assert.Same(ffmpegProcessFactory, ffmpegProcessFactory2);
    }

    [Fact]
    public void FfmpegFactory2()
    {
        var kernel = KernelUtils.Init();

        var ffmpegFactory = kernel.GetInstance<IFfmpegProcessFactory>();
        Assert.NotNull(ffmpegFactory);

        var instance = ffmpegFactory.CreateFfmpegProcess();
        Assert.NotNull(instance);

        var instance2 = ffmpegFactory.CreateFfmpegProcess();
        Assert.NotNull(instance2);

        Assert.NotSame(instance, instance2);
    }

    [Fact]
    public void Restart()
    {
        var kernel = KernelUtils.Init();

        var restart = kernel.GetInstance<IGameRestart>();
        Assert.NotNull(restart);

        var restart2 = kernel.GetInstance<IGameRestart>();
        Assert.NotNull(restart2);

        Assert.Same(restart, restart2);
    }

    [Fact]
    public void VirtualEnvironment()
    {
        var kernel = KernelUtils.Init();

        var virtualEnvironment = kernel.GetInstance<VirtualEnvController>();
        Assert.NotNull(virtualEnvironment);

        var virtualEnvironment2 = kernel.GetInstance<VirtualEnvController>();
        Assert.NotNull(virtualEnvironment2);

        Assert.Same(virtualEnvironment, virtualEnvironment2);
    }

    [Fact]
    public void SyncFixedUpdate()
    {
        var kernel = KernelUtils.Init();

        var syncFixedUpdate = kernel.GetInstance<ISyncFixedUpdateCycle>();
        Assert.NotNull(syncFixedUpdate);

        var syncFixedUpdate2 = kernel.GetInstance<ISyncFixedUpdateCycle>();
        Assert.NotNull(syncFixedUpdate2);

        Assert.Same(syncFixedUpdate, syncFixedUpdate2);
    }

    [Fact]
    public void MonoBehaviourController()
    {
        var kernel = KernelUtils.Init();

        var monoBehaviourController = kernel.GetInstance<IMonoBehaviourController>();
        Assert.NotNull(monoBehaviourController);

        var monoBehaviourController2 = kernel.GetInstance<IMonoBehaviourController>();
        Assert.NotNull(monoBehaviourController2);

        Assert.Same(monoBehaviourController, monoBehaviourController2);
    }

    [Fact]
    public void Logger()
    {
        var kernel = KernelUtils.Init();

        var logger = kernel.GetInstance<ILogger>();
        Assert.NotNull(logger);

        var logger2 = kernel.GetInstance<ILogger>();
        Assert.NotNull(logger2);

        Assert.Same(logger, logger2);
    }

    [Fact]
    public void OnGameRestart()
    {
        var kernel = KernelUtils.Init();

        var onGameRestart = kernel.GetAllInstances<IOnGameRestart>();
        Assert.NotNull(onGameRestart);

        var onGameRestart2 = kernel.GetAllInstances<IOnGameRestart>();
        Assert.NotNull(onGameRestart2);

        Assert.Equal(onGameRestart, onGameRestart2);
    }

    [Fact]
    public void StaticFieldManipulator()
    {
        var kernel = KernelUtils.Init();

        var staticFieldManipulator = kernel.GetInstance<IStaticFieldManipulator>();
        Assert.NotNull(staticFieldManipulator);

        var staticFieldManipulator2 = kernel.GetInstance<IStaticFieldManipulator>();
        Assert.NotNull(staticFieldManipulator2);

        Assert.Same(staticFieldManipulator, staticFieldManipulator2);
    }

    [Fact]
    public void OnGameRestartResume()
    {
        var kernel = KernelUtils.Init();

        var onGameRestartResume = kernel.GetAllInstances<IOnGameRestartResume>();
        Assert.NotNull(onGameRestartResume);
    }

    [Fact]
    public void OnPreGameRestart()
    {
        var kernel = KernelUtils.Init();

        var onGameRestartPause = kernel.GetAllInstances<IOnPreGameRestart>();
        Assert.NotNull(onGameRestartPause);

        var onGameRestartPause2 = kernel.GetAllInstances<IOnPreGameRestart>();
        Assert.NotNull(onGameRestartPause2);

        Assert.Equal(onGameRestartPause, onGameRestartPause2);
    }

    [Fact]
    public void PatchReverseInvoker()
    {
        var kernel = KernelUtils.Init();

        var patchReverseInvoker = kernel.GetInstance<IPatchReverseInvoker>();
        Assert.NotNull(patchReverseInvoker);

        var patchReverseInvoker2 = kernel.GetInstance<IPatchReverseInvoker>();
        Assert.NotNull(patchReverseInvoker2);

        Assert.Same(patchReverseInvoker, patchReverseInvoker2);
    }

    [Fact]
    public void EnvEngineMethod()
    {
        var kernel = KernelUtils.Init();

        var env = kernel.GetInstance<KernelUtils.Env>();
        Assert.NotNull(env);

        var env2 = kernel.GetInstance<KernelUtils.Env>();
        Assert.NotNull(env2);

        Assert.Same(env, env2);

        var scriptArg = new ExplicitArguments();
        scriptArg.Set(typeof(Script), null);
        var engine = kernel.GetInstance<IMovieEngine>(scriptArg);
        var env3 = kernel.GetInstance<IEngineModuleClassesFactory>().GetAll(engine)
            .OfType<KernelUtils.Env>().Single();
        Assert.NotNull(env3);

        Assert.Same(env, env3);

        var env4 = kernel.GetAllInstances<IOnLastUpdateUnconditional>().OfType<KernelUtils.Env>().Single();
        Assert.NotNull(env4);

        Assert.Same(env, env4);
    }

    [Fact]
    public void TestPriority()
    {
        var kernel = KernelUtils.Init();

        var updates = kernel.GetAllInstances<IOnPreUpdateActual>().ToList();
        Assert.NotNull(updates);

        var indexOfTestPriority = updates.FindIndex(x => x is KernelUtils.TestPriority);
        var indexOfTestPriority2 = updates.FindIndex(x => x is KernelUtils.TestPriority2);
        // var indexOfMovieRunner = inputUpdates.FindIndex(x => x is Implementations.Movie.MovieRunner);

        Assert.True(indexOfTestPriority < indexOfTestPriority2);
        // Assert.True(indexOfTestPriority < indexOfMovieRunner);
    }
}