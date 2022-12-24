using System;
using UniTASPlugin.GameInfo;
using UniTASPlugin.Logger;
using UniTASPlugin.Patches.PatchTypes;

namespace UniTASPlugin.Patches.PatchProcessor;

public class MscorlibPatchProcessor : PatchProcessor
{
    private readonly IGameInfo _gameInfo;

    public MscorlibPatchProcessor(ILogger logger, IGameInfo gameInfo) : base(logger)
    {
        _gameInfo = gameInfo;
    }

    protected override Type TargetPatchType => typeof(MscorlibPatch);
    protected override string Version => _gameInfo.MscorlibVersion;
}