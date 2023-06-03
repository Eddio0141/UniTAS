using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.StaticServices;

namespace UniTAS.Patcher.Implementations.NewInputSystem;

[Singleton]
public class NewInputSystemExists : INewInputSystemExists
{
    public bool HasInputSystem => NewInputSystemState.NewInputSystemExists;
}