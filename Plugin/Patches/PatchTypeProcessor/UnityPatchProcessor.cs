using System;
using UniTASPlugin.GameInfo;
using UniTASPlugin.Logger;
using UniTASPlugin.Patches.PatchTypes;

namespace UniTASPlugin.Patches.PatchTypeProcessor;

public class UnityPatchProcessor : PatchProcessor
{
    private readonly IGameInfo _gameInfo;

    public UnityPatchProcessor(ILogger logger, IGameInfo gameInfo) : base(logger)
    {
        _gameInfo = gameInfo;
    }

    protected override Type TargetPatchType => typeof(UnityPatch);
    protected override string Version => _gameInfo.UnityVersion;
}