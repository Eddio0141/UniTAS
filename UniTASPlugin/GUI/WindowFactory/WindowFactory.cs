namespace UniTASPlugin.GUI.WindowFactory;

public class WindowFactory : IWindowFactory
{
    public T Create<T>() where T : Window
    {
        return (T)System.Activator.CreateInstance(typeof(T));
    }
}