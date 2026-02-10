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
        // new SteamAPIPatch(),
        new SerializationCallbackPatch(),
        new CoroutinePatch(),
        new AsyncOperationPatch(),
        new FunctionCallTrace() // this must run last, it hooks logs on start and ret
    ];
}