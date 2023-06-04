using UniTAS.Patcher.Interfaces.GUI;

namespace UniTAS.Patcher.Services.GUI;

public interface IWindowFactory
{
    T Create<T>(string windowName = null) where T : Window;
}