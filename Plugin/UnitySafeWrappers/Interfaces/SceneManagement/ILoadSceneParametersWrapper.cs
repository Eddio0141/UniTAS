using UniTASPlugin.UnitySafeWrappers.Wrappers;

namespace UniTASPlugin.UnitySafeWrappers.Interfaces.SceneManagement;

public interface ILoadSceneParametersWrapper
{
    void CreateInstance();
    object Instance { get; set; }
    LoadSceneMode? LoadSceneMode { get; set; }
    LocalPhysicsMode? LocalPhysicsMode { get; set; }
}