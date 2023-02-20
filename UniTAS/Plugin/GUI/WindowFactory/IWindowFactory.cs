namespace UniTAS.Plugin.GUI.WindowFactory;

public interface IWindowFactory
{
    T Create<T>(string windowName = null) where T : Window;
}