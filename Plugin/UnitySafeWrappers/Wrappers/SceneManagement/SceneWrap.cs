using HarmonyLib;
using UniTASPlugin.UnitySafeWrappers.Interfaces.SceneManagement;

namespace UniTASPlugin.UnitySafeWrappers.Wrappers.SceneManagement;

// ReSharper disable once UnusedType.Global
public class SceneWrap : ISceneWrap
{
    private Traverse _instanceTraverse;
    private object _instance;

    private const string BuildIndexField = "buildIndex";
    private const string NameField = "name";

    public object Instance
    {
        get => _instance;
        set
        {
            _instanceTraverse = Traverse.Create(value);
            _instance = value;
        }
    }

    public int BuildIndex => _instanceTraverse.Property(BuildIndexField).GetValue<int>();
    public string Name => _instanceTraverse.Property(NameField).GetValue<string>();
}