namespace UniTASPlugin.UnitySafeWrappers.Interfaces.SceneManagement;

public interface ISceneWrap
{
    object Instance { get; set; }
    int BuildIndex { get; }
    string Name { get; }
}