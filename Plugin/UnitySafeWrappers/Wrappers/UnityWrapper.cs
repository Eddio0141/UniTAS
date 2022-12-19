using UniTASPlugin.UnitySafeWrappers.Interfaces;

namespace UniTASPlugin.UnitySafeWrappers.Wrappers;

public class UnityWrapper : IUnityWrapper
{
    public IObjectWrapper Object { get; }
    public IMonoBehaviourWrapper MonoBehaviour { get; }

    public UnityWrapper(IObjectWrapper objectWrapper, IMonoBehaviourWrapper monoBehaviourWrapper)
    {
        Object = objectWrapper;
        MonoBehaviour = monoBehaviourWrapper;
    }
}