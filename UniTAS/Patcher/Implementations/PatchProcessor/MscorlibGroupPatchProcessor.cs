using System;
using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.Patches.PatchGroups;
using UniTAS.Patcher.Interfaces.Patches.PatchProcessor;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations.PatchProcessor;

public class MscorlibGroupPatchProcessor : GroupPatchProcessor
{
    private readonly IGameInfo _gameInfo;

    public MscorlibGroupPatchProcessor(ILogger logger, IGameInfo gameInfo) : base(logger)
    {
        _gameInfo = gameInfo;
    }

    protected override Type TargetPatchType => typeof(MscorlibPatch);

    protected override IEnumerable<int> ChoosePatch(ModulePatchType patchType, PatchGroup[] patchGroups)
    {
        var mscorlibVersion = VersionStringToNumber(_gameInfo.MscorlibVersion);
        var netstandardVersion = _gameInfo.NetStandardVersion;
        var net20Subset = _gameInfo.Net20Subset;

        if (patchType.PatchAllGroups)
        {
            for (var i = 0; i < patchGroups.Length; i++)
            {
                var patchGroup = patchGroups[i];
                var mscorlibPatchGroup = (MscorlibPatchGroup)patchGroup;

                if (mscorlibPatchGroup.NetStandardVersion != null &&
                    netstandardVersion != mscorlibPatchGroup.NetStandardVersion) continue;

                if (mscorlibPatchGroup.Net20Subset != null &&
                    net20Subset != mscorlibPatchGroup.Net20Subset) continue;

                var versionStart = VersionStringToNumber(mscorlibPatchGroup.RangeStart);
                var versionEnd = VersionStringToNumber(mscorlibPatchGroup.RangeEnd, ulong.MaxValue);

                // patch group version check
                if (versionStart <= mscorlibVersion && mscorlibVersion <= versionEnd)
                {
                    yield return i;
                }
            }

            yield break;
        }

        for (var i = 0; i < patchGroups.Length; i++)
        {
            var patchGroup = patchGroups[i];
            var mscorlibPatchGroup = (MscorlibPatchGroup)patchGroup;

            var versionStart = VersionStringToNumber(mscorlibPatchGroup.RangeStart);
            var versionEnd = VersionStringToNumber(mscorlibPatchGroup.RangeEnd, ulong.MaxValue);

            if (versionStart > mscorlibVersion || mscorlibVersion > versionEnd ||
                (mscorlibPatchGroup.NetStandardVersion != null &&
                 mscorlibPatchGroup.NetStandardVersion != netstandardVersion) ||
                (mscorlibPatchGroup.Net20Subset != null &&
                 mscorlibPatchGroup.Net20Subset.Value != net20Subset)) continue;

            yield return i;
            yield break;
        }
    }
}