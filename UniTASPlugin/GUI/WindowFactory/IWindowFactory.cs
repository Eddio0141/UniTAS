namespace UniTASPlugin.GUI.WindowFactory;

public interface IWindowFactory
{
    T Create<T>() where T : Window;
}