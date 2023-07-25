namespace UniTAS.Patcher.Models.DependencyInjection;

public enum RegisterPriority
{
    // the higher you place it, the lower the value, the earlier it gets processed in the registering
    FirstUpdateSkipOnRestart,
    TimeEnv,
    MovieRunner,
    Default,
    FrameAdvancing,
    CoroutineHandler,
    ToolBar,
    InfoPrintAndWelcome
}