using UniTAS.Plugin.UnitySafeWrappers;

namespace UniTAS.Plugin.Services.UnitySafeWrappers;

/// <summary>
/// A factory for creating unity instance wraps with extra functionality or for creating types that may or may not be present in the current unity version
/// </summary>
public interface IUnityInstanceWrapFactory
{
    /// <summary>
    /// A wrapped instance via wrapper type T
    /// </summary>
    /// <param name="instance">Actual instance to pass to the wrapper</param>
    /// <typeparam name="T">Type of the wrap type</typeparam>
    /// <returns>Wrapped instance</returns>
    T Create<T>(object instance) where T : UnityInstanceWrap;

    /// <summary>
    /// Creates a new instance of the type T
    /// It will choose the constructor with the most parameters that can be satisfied by the args
    /// </summary>
    /// <param name="args">Arguments to pass to the constructor</param>
    /// <typeparam name="T">Type of the wrap type</typeparam>
    /// <returns>Wrapped instance</returns>
    T CreateNew<T>(params object[] args) where T : UnityInstanceWrap;
}