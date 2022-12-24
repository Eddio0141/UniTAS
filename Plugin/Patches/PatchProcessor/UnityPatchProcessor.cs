using System;
using System.Collections.Generic;
using UniTASPlugin.GameInfo;
using UniTASPlugin.Logger;
using UniTASPlugin.Patches.PatchGroups;
using UniTASPlugin.Patches.PatchTypes;

namespace UniTASPlugin.Patches.PatchProcessor;

public class UnityPatchProcessor : PatchProcessor
{
    private readonly IGameInfo _gameInfo;

    public UnityPatchProcessor(ILogger logger, IGameInfo gameInfo) : base(logger)
    {
        _gameInfo = gameInfo;
    }

    protected override IEnumerable<int> ChoosePatch(PatchType patchType, PatchGroup[] patchGroups)
    {
        var version = VersionStringToNumber(_gameInfo.UnityVersion);

        if (patchType.PatchAllGroups)
        {
            for (var i = 0; i < patchGroups.Length; i++)
            {
                var group = patchGroups[i];
                var groupUnityPatch = (UnityPatchGroup)group;

                ulong? versionStart = groupUnityPatch.RangeStart == null
                    ? null
                    : VersionStringToNumber(groupUnityPatch.RangeStart);
                ulong? versionEnd = groupUnityPatch.RangeEnd == null
                    ? null
                    : VersionStringToNumber(groupUnityPatch.RangeEnd);

                // patch group version check
                if (versionStart == null || versionEnd == null ||
                    (versionStart <= version && version <= versionEnd))
                {
                    yield return i;
                }
            }

            yield break;
        }

        for (var i = 0; i < patchGroups.Length; i++)
        {
            var patchGroup = patchGroups[i];
            var unityPatchGroup = (UnityPatchGroup)patchGroup;
            if (unityPatchGroup.RangeStart == null || unityPatchGroup.RangeEnd == null) continue;

            var versionStart = VersionStringToNumber(unityPatchGroup.RangeStart);
            var versionEnd = VersionStringToNumber(unityPatchGroup.RangeEnd);

            if (versionStart > version || version > versionEnd) continue;

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

    protected override Type TargetPatchType => typeof(UnityPatch);
}