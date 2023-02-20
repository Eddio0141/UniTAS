using UniTASPlugin.FixedUpdateSync;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.GameRestart;
using UniTASPlugin.GameRestart.EventInterfaces;
using UniTASPlugin.Logger;
using UniTASPlugin.MonoBehaviourController;
using UniTASPlugin.ReverseInvoker;
using UniTASPlugin.StaticFieldStorage;
using UniTASPlugin.Trackers.DontDestroyOnLoadTracker;
using UniTASPlugin.UnitySafeWrappers.Interfaces;

namespace UniTASPlugin.Tests;

public class KernelTests
{
    [Fact]
    public void Restart()
    {
        var kernel = ContainerRegister.Init();

        var restart = kernel.GetInstance<IGameRestart>();
        Assert.NotNull(restart);

        var restart2 = kernel.GetInstance<IGameRestart>();
        Assert.NotNull(restart2);

        Assert.Equal(restart, restart2);
    }

    [Fact]
    public void VirtualEnvironment()
    {
        var kernel = ContainerRegister.Init();

        var virtualEnvironment = kernel.GetInstance<VirtualEnvironment>();
        Assert.NotNull(virtualEnvironment);

        var virtualEnvironment2 = kernel.GetInstance<VirtualEnvironment>();
        Assert.NotNull(virtualEnvironment2);

        Assert.Equal(virtualEnvironment, virtualEnvironment2);
    }

    [Fact]
    public void SyncFixedUpdate()
    {
        var kernel = ContainerRegister.Init();

        var syncFixedUpdate = kernel.GetInstance<ISyncFixedUpdate>();
        Assert.NotNull(syncFixedUpdate);

        var syncFixedUpdate2 = kernel.GetInstance<ISyncFixedUpdate>();
        Assert.NotNull(syncFixedUpdate2);

        Assert.Equal(syncFixedUpdate, syncFixedUpdate2);
    }

    [Fact]
    public void UnityWrapper()
    {
        var kernel = ContainerRegister.Init();

        var unityWrapper = kernel.GetInstance<IUnityWrapper>();
        Assert.NotNull(unityWrapper);

        var unityWrapper2 = kernel.GetInstance<IUnityWrapper>();
        Assert.NotNull(unityWrapper2);

        Assert.Equal(unityWrapper, unityWrapper2);
    }

    [Fact]
    public void MonoBehaviourController()
    {
        var kernel = ContainerRegister.Init();

        var monoBehaviourController = kernel.GetInstance<IMonoBehaviourController>();
        Assert.NotNull(monoBehaviourController);

        var monoBehaviourController2 = kernel.GetInstance<IMonoBehaviourController>();
        Assert.NotNull(monoBehaviourController2);

        Assert.Equal(monoBehaviourController, monoBehaviourController2);
    }

    [Fact]
    public void Logger()
    {
        var kernel = ContainerRegister.Init();

        var logger = kernel.GetInstance<ILogger>();
        Assert.NotNull(logger);

        var logger2 = kernel.GetInstance<ILogger>();
        Assert.NotNull(logger2);

        Assert.Equal(logger, logger2);
    }

    [Fact]
    public void OnGameRestart()
    {
        var kernel = ContainerRegister.Init();

        var onGameRestart = kernel.GetAllInstances<IOnGameRestart>();
        Assert.NotNull(onGameRestart);

        var onGameRestart2 = kernel.GetAllInstances<IOnGameRestart>();
        Assert.NotNull(onGameRestart2);

        Assert.Equal(onGameRestart, onGameRestart2);
    }

    [Fact]
    public void StaticFieldManipulator()
    {
        var kernel = ContainerRegister.Init();

        var staticFieldManipulator = kernel.GetInstance<IStaticFieldManipulator>();
        Assert.NotNull(staticFieldManipulator);

        var staticFieldManipulator2 = kernel.GetInstance<IStaticFieldManipulator>();
        Assert.NotNull(staticFieldManipulator2);

        Assert.Equal(staticFieldManipulator, staticFieldManipulator2);
    }

    [Fact]
    public void DontDestroyOnLoadInfo()
    {
        var kernel = ContainerRegister.Init();

        var dontDestroyOnLoadInfo = kernel.GetInstance<IDontDestroyOnLoadInfo>();
        Assert.NotNull(dontDestroyOnLoadInfo);

        var dontDestroyOnLoadInfo2 = kernel.GetInstance<IDontDestroyOnLoadInfo>();
        Assert.NotNull(dontDestroyOnLoadInfo2);

        Assert.Equal(dontDestroyOnLoadInfo, dontDestroyOnLoadInfo2);
    }

    [Fact]
    public void OnGameRestartResume()
    {
        var kernel = ContainerRegister.Init();

        var onGameRestartResume = kernel.GetAllInstances<IOnGameRestartResume>();
        Assert.NotNull(onGameRestartResume);

        var onGameRestartResume2 = kernel.GetAllInstances<IOnGameRestartResume>();
        Assert.NotNull(onGameRestartResume2);

        Assert.Equal(onGameRestartResume, onGameRestartResume2);
    }

    [Fact]
    public void OnPreGameRestart()
    {
        var kernel = ContainerRegister.Init();

        var onGameRestartPause = kernel.GetAllInstances<IOnPreGameRestart>();
        Assert.NotNull(onGameRestartPause);

        var onGameRestartPause2 = kernel.GetAllInstances<IOnPreGameRestart>();
        Assert.NotNull(onGameRestartPause2);

        Assert.Equal(onGameRestartPause, onGameRestartPause2);
    }

    [Fact]
    public void PatchReverseInvoker()
    {
        var kernel = ContainerRegister.Init();

        var patchReverseInvoker = kernel.GetInstance<IPatchReverseInvoker>();
        Assert.NotNull(patchReverseInvoker);

        var patchReverseInvoker2 = kernel.GetInstance<IPatchReverseInvoker>();
        Assert.NotNull(patchReverseInvoker2);

        Assert.Equal(patchReverseInvoker, patchReverseInvoker2);
    }
}