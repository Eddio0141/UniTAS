using UniTAS.Plugin.Interfaces.GUI;

namespace UniTAS.Plugin.Services.GUI;

public interface IWindowFactory
{
    T Create<T>(string windowName = null) where T : Window;
}