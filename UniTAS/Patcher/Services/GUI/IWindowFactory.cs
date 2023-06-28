using UniTAS.Patcher.Interfaces.GUI;

namespace UniTAS.Patcher.Services.GUI;

public interface IWindowFactory
{
    T Create<T>() where T : Window;
}