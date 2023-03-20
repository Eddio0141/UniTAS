namespace UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;

public interface INativeArray<in T>
{
    void ToArray(T[] array);
}