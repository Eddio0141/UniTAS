using BepInEx;

namespace UniTASPlugin;

[BepInPlugin("____UniTASPlugin", "UniTASPlugin", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
}
