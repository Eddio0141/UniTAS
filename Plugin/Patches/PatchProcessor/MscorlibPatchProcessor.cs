using System;
using System.Collections.Generic;
using UniTASPlugin.GameInfo;
using UniTASPlugin.Logger;
using UniTASPlugin.Patches.PatchGroups;
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

    protected override IEnumerable<int> ChoosePatch(PatchType patchType, PatchGroup[] patchGroups)
    {
        var mscorlibVersion = VersionStringToNumber(_gameInfo.MscorlibVersion);
        var netstandardVersion = _gameInfo.NetStandardVersion;

        if (patchType.PatchAllGroups)
        {
            for (var i = 0; i < patchGroups.Length; i++)
            {
                var patchGroup = patchGroups[i];
                var mscorlibPatchGroup = (MscorlibPatchGroup)patchGroup;

                if (mscorlibPatchGroup.NetStandardVersion != null &&
                    netstandardVersion != mscorlibPatchGroup.NetStandardVersion) continue;

                var versionStart = VersionStringToNumber(mscorlibPatchGroup.RangeStart);
                var versionEnd = VersionStringToNumber(mscorlibPatchGroup.RangeEnd, ulong.MaxValue);

                // patch group version check
                if (versionStart <= mscorlibVersion && mscorlibVersion <= versionEnd)
                {
                    yield return i;
                }
            }
        }

        for (var i = 0; i < patchGroups.Length; i++)
        {
            var patchGroup = patchGroups[i];
            var mscorlibPatchGroup = (MscorlibPatchGroup)patchGroup;

            var versionStart = VersionStringToNumber(mscorlibPatchGroup.RangeStart);
            var versionEnd = VersionStringToNumber(mscorlibPatchGroup.RangeEnd, ulong.MaxValue);

            if (versionStart > mscorlibVersion || mscorlibVersion > versionEnd ||
                (mscorlibPatchGroup.NetStandardVersion != null &&
                 mscorlibPatchGroup.NetStandardVersion != netstandardVersion)) continue;

            yield return i;
            yield break;
        }
    }
}