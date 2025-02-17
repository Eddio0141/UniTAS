namespace UniTAS.Patcher.Models.DependencyInjection;

public enum RegisterPriority
{
    // the higher you place it, the lower the value, the earlier it gets processed in the registering
    MovieRunner,
    RuntimeTestProcessor,
    FrameAdvancing,
    StaticFieldStorage, // must be before InvokeAllAfterDeserialization (Default)
    Default,
    CoroutineHandler,
    ToolBar,
    KeyBindsWindow,
    InfoPrintAndWelcome,
    RemoteControl
}