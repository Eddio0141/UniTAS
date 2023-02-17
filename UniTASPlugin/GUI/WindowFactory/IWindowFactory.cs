namespace UniTASPlugin.GUI.WindowFactory;

public interface IWindowFactory
{
    T Create<T>(string windowName = null) where T : Window;
}