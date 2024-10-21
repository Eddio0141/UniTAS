using UniTAS.Patcher.Interfaces.GUI;

namespace UniTAS.Patcher.Services.GUI;

public interface IGUIComponentFactory
{
    T CreateComponent<T>() where T : IGUIComponent;
}