using UniTASPlugin.UnitySafeWrappers.Wrappers;

namespace UniTASPlugin.UnitySafeWrappers.Interfaces.SceneManagement;

public interface ILoadSceneParametersWrapper
{
    object Instance { get; }
    LoadSceneMode? LoadSceneMode { get; set; }
    LocalPhysicsMode? LocalPhysicsMode { get; set; }
}