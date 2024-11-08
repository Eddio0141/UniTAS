using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.Patches.Preloader;

namespace UniTAS.Patcher.Implementations;

public class PreloadPatcherProcessor
{
    public PreloadPatcher[] PreloadPatchers { get; } =
    [
        new MonoBehaviourPatch(),
        new StaticCtorHeaders(),
        new UnityInitInvoke(),
        new FinalizeSuppressionPatch(),
        new SteamAPIPatch(),
        new SerializationCallbackPatch()
    ];
}