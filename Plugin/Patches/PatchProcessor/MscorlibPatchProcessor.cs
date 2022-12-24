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
        var isNetstandard21 = netstandardVersion == "2.1.0.0";

        if (patchType.PatchAllGroups)
        {
            for (var i = 0; i < patchGroups.Length; i++)
            {
                var patchGroup = patchGroups[i];
                var mscorlibPatchGroup = (MscorlibPatchGroup)patchGroup;

                if (isNetstandard21 != mscorlibPatchGroup.NetStandard21) continue;

                ulong? versionStart = mscorlibPatchGroup.RangeStart == null
                    ? null
                    : VersionStringToNumber(mscorlibPatchGroup.RangeStart);
                ulong? versionEnd = mscorlibPatchGroup.RangeEnd == null
                    ? null
                    : VersionStringToNumber(mscorlibPatchGroup.RangeEnd);

                // patch group version check
                if (versionStart == null || versionEnd == null ||
                    (versionStart <= mscorlibVersion && mscorlibVersion <= versionEnd))
                {
                    yield return i;
                }
            }
        }

        for (var i = 0; i < patchGroups.Length; i++)
        {
            var patchGroup = patchGroups[i];
            var mscorlibPatchGroup = (MscorlibPatchGroup)patchGroup;
            if (mscorlibPatchGroup.RangeStart == null || mscorlibPatchGroup.RangeEnd == null) continue;

            var versionStart = VersionStringToNumber(mscorlibPatchGroup.RangeStart);
            var versionEnd = VersionStringToNumber(mscorlibPatchGroup.RangeEnd);

            if (versionStart > mscorlibVersion || mscorlibVersion > versionEnd ||
                mscorlibPatchGroup.NetStandard21 != isNetstandard21) continue;

            yield return i;
            yield break;
        }
    }

    private static ulong VersionStringToNumber(string version)
    {
        var versionParts = version.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        var versionNumber = 0ul;
        var multiplier = (ulong)Math.Pow(10, versionParts.Length - 1);
        foreach (var versionPart in versionParts)
        {
            if (!ulong.TryParse(versionPart, out var versionPartNumber))
            {
                versionPartNumber = 0;
            }

            versionNumber += versionPartNumber * multiplier;
            multiplier /= 10;
        }

        return versionNumber;
    }
}