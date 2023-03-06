using UniTAS.Plugin.FixedUpdateSync;
using UniTAS.Plugin.GameEnvironment;
using UniTAS.Plugin.GameInitialRestart;
using UniTAS.Plugin.GameRestart;
using UniTAS.Plugin.GameRestart.EventInterfaces;
using UniTAS.Plugin.Logger;
using UniTAS.Plugin.MonoBehaviourController;
using UniTAS.Plugin.ReverseInvoker;
using UniTAS.Plugin.StaticFieldStorage;
using UniTAS.Plugin.UnitySafeWrappers.Interfaces;

namespace UniTAS.Plugin.Tests.Kernel;

public class KernelTests
{
    [Fact]
    public void Restart()
    {
        var kernel = KernelUtils.Init();

        var restart = kernel.GetInstance<IGameRestart>();
        Assert.NotNull(restart);

        var restart2 = kernel.GetInstance<IGameRestart>();
        Assert.NotNull(restart2);

        Assert.Equal(restart, restart2);
    }

    [Fact]
    public void VirtualEnvironment()
    {
        var kernel = KernelUtils.Init();

        var virtualEnvironment = kernel.GetInstance<VirtualEnvironment>();
        Assert.NotNull(virtualEnvironment);

        var virtualEnvironment2 = kernel.GetInstance<VirtualEnvironment>();
        Assert.NotNull(virtualEnvironment2);

        Assert.Equal(virtualEnvironment, virtualEnvironment2);
    }

    [Fact]
    public void SyncFixedUpdate()
    {
        var kernel = KernelUtils.Init();

        var syncFixedUpdate = kernel.GetInstance<ISyncFixedUpdate>();
        Assert.NotNull(syncFixedUpdate);

        var syncFixedUpdate2 = kernel.GetInstance<ISyncFixedUpdate>();
        Assert.NotNull(syncFixedUpdate2);

        Assert.Equal(syncFixedUpdate, syncFixedUpdate2);
    }

    [Fact]
    public void UnityWrapper()
    {
        var kernel = KernelUtils.Init();

        var unityWrapper = kernel.GetInstance<IUnityWrapper>();
        Assert.NotNull(unityWrapper);

        var unityWrapper2 = kernel.GetInstance<IUnityWrapper>();
        Assert.NotNull(unityWrapper2);

        Assert.Equal(unityWrapper, unityWrapper2);
    }

    [Fact]
    public void MonoBehaviourController()
    {
        var kernel = KernelUtils.Init();

        var monoBehaviourController = kernel.GetInstance<IMonoBehaviourController>();
        Assert.NotNull(monoBehaviourController);

        var monoBehaviourController2 = kernel.GetInstance<IMonoBehaviourController>();
        Assert.NotNull(monoBehaviourController2);

        Assert.Equal(monoBehaviourController, monoBehaviourController2);
    }

    [Fact]
    public void Logger()
    {
        var kernel = KernelUtils.Init();

        var logger = kernel.GetInstance<ILogger>();
        Assert.NotNull(logger);

        var logger2 = kernel.GetInstance<ILogger>();
        Assert.NotNull(logger2);

        Assert.Equal(logger, logger2);
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

        Assert.Equal(staticFieldManipulator, staticFieldManipulator2);
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

        Assert.Equal(patchReverseInvoker, patchReverseInvoker2);
    }

    [Fact]
    public void GameInitialRestart()
    {
        var kernel = KernelUtils.Init();

        var gameInitialRestart = kernel.GetInstance<IGameInitialRestart>();
        Assert.NotNull(gameInitialRestart);

        var gameInitialRestart2 = kernel.GetInstance<IGameInitialRestart>();
        Assert.NotNull(gameInitialRestart2);

        Assert.Equal(gameInitialRestart, gameInitialRestart2);

        var gameRestart = kernel.GetInstance<IGameRestart>();

        // reference should be different
        Assert.True(gameInitialRestart != gameRestart);
    }
}