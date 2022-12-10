namespace UniTASFunkyInjector;

public static class ComponentStarter
{
    public static ComponentBuilder<T> For<T>()
    {
        return new ComponentBuilder<T>();
    }
}