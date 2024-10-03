using System.Reflection;

namespace UniTAS.Patcher.Services;

public interface ITryFreeMalloc
{
    /// <summary>
    /// Tries to free the memory of the given field
    /// </summary>
    void TryFree(object instance, FieldInfo fieldInfo);
}