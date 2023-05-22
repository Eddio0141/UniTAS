using UniTAS.Patcher.Shared;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Services.InputSystemOverride;

namespace UniTAS.Plugin.Implementations.NewInputSystem;

[Singleton]
public class NewInputSystemExists : INewInputSystemExists
{
    public bool HasInputSystem => NewInputSystemState.NewInputSystemExists;
}