namespace UniTAS.Plugin.UnitySafeWrappers.Interfaces;

public interface INativeArray<in T>
{
    void ToArray(T[] array);
}